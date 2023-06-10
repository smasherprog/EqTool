using Autofac;
using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HttpClient httpclient = new HttpClient();

        private Autofac.IContainer container;
        private System.Windows.Forms.NotifyIcon SystemTrayIcon;

        private System.Windows.Forms.MenuItem MapMenuItem;
        private System.Windows.Forms.MenuItem SpellsMenuItem;
        private System.Windows.Forms.MenuItem DpsMeterMenuItem;
        private System.Windows.Forms.MenuItem SettingsMenuItem;
        private System.Windows.Forms.MenuItem MobInfoMenuItem;
        private LogParser logParser => container.Resolve<LogParser>();
        private PlayerTrackerService PlayerTrackerService;
        private EQToolSettings EQToolSettings => container.Resolve<EQToolSettings>();
        public static List<Window> WindowList = new List<Window>();

        private bool WaitForEQToolToStop()
        {
            var counter = 0;
            int count;
            do
            {
                count = Process.GetProcessesByName("eqtool").Count();
                if (counter++ > 6)
                {
                    return false;
                }
                Debug.WriteLine($"Waiting for eqtool {count} on counter {counter}");
                Thread.Sleep(1000);
            }
            while (count != 1);
            return true;
        }

        public void CheckForUpdates(object sender, EventArgs e)
        {
            new UpdateService().CheckForUpdates(Version);
        }
        public enum BuildType
        {
            Release,
            Debug,
            Test,
            Beta
        }

        public enum EventType
        {
            Error,
            StartUp,
            Update,
            OpenMap,
            OpenMobInfo,
            OpenDPS,
            OpenTriggers
        }

        public class ExceptionRequest
        {
            public string Version { get; set; }
            public string Message { get; set; }
            public EventType EventType { get; set; }
            public BuildType BuildType { get; set; }
        }

        public static void LogUnhandledException(Exception exception, string source)
        {
            var build = BuildType.Release;
#if TEST
            build =  BuildType.Test;
#elif DEBUG
            build = BuildType.Debug;
#elif BETA
            build = BuildType.Beta;
#endif
            try
            {
                var msg = new ExceptionRequest
                {
                    Version = Version,
                    Message = $"Unhandled exception ({source}) {exception}",
                    EventType = EventType.Error,
                    BuildType = build
                };
                var msagasjson = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                var content = new StringContent(msagasjson, Encoding.UTF8, "application/json");
                var result = httpclient.PostAsync("https://pigparse.azurewebsites.net/api/eqtool/exception", content).Result;
            }
            catch { }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            };
        }

        private bool ShouldShutDownDueToNoWriteAccess()
        {
            try
            {
                File.Delete("test.json");
            }
            catch { }
            try
            {
                File.WriteAllText("test.json", "test");
            }
            catch (UnauthorizedAccessException)
            {
                _ = MessageBox.Show("EQTool is running from a directory where it does not have permission to save settings. Please, move it to a folder where it can write!", "EQTool Permissions!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            try
            {
                File.Delete("test.json");
            }
            catch { }
            return false;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (ShouldShutDownDueToNoWriteAccess())
            {
                App.Current.Shutdown();
                return;
            }
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SetupExceptionHandling();
            if (!WaitForEQToolToStop())
            {
                MessageBox.Show("Another EQTool is currently running. You must shut that one down first!", "Multiple EQTools running!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }

            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            var updateservice = new UpdateService();
            var did_update = updateservice.ApplyUpdate(e.Args.FirstOrDefault());
            if (did_update == UpdateService.UpdateStatus.UpdatesApplied)
            {
                return;
            }
            else if (did_update == UpdateService.UpdateStatus.NoUpdateApplied)
            {
#if !DEBUG
                updateservice.CheckForUpdates(Version);
#endif
            }
            InitStuff();
        }

        private void InitStuff()
        {
            container = DI.Init();
            container.Resolve<LoggingService>().Log(string.Empty, EventType.StartUp);
            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", ToggleSettingsWindow);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Spells", ToggleSpellsWindow);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", ToggleMapWindow);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", ToggleDPSWindow);
            MobInfoMenuItem = new System.Windows.Forms.MenuItem("Mob Info", ToggleMobInfoWindow);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Why the Pig?", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Check for Update", CheckForUpdates);
            var versionstring = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var beta = false;

#if BETA || DEBUG
            beta = true;
#endif

            var logo = EQTool.Properties.Resources.logo;
            if (beta)
            {
                versionstring = "Beta-" + versionstring;
                logo = EQTool.Properties.Resources.sickpic;
            }

            var version = new System.Windows.Forms.MenuItem(versionstring)
            {
                Enabled = false
            };
            ToggleMenuButtons(false);
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = logo,
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                {
                     whythepig,
                    DpsMeterMenuItem,
                    MapMenuItem,
                    SpellsMenuItem,
                    MobInfoMenuItem,
                    SettingsMenuItem,
                    gitHubMenuItem,
                    updates,
                    version,
                    new System.Windows.Forms.MenuItem("Exit", OnExit)
                }),
            };

            var hasvalideqdir = FindEq.IsProject1999Folder(EQToolSettings.DefaultEqDirectory);
            if (!hasvalideqdir || FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == false)
            {
                if (!hasvalideqdir)
                {
                    EQToolSettings.DefaultEqDirectory = string.Empty;
                }
                OpenSettingsWindow();
            }
            else
            {
                ToggleMenuButtons(true);
                if (!EQToolSettings.SpellWindowState.Closed)
                {
                    OpenSpellsWindow();
                }
                if (!EQToolSettings.DpsWindowState.Closed)
                {
                    OpenDPSWindow();
                }
                if (!EQToolSettings.MapWindowState.Closed)
                {
                    OpenMapWindow();
                }
                if (!EQToolSettings.MobWindowState.Closed)
                {
                    OpenMobInfoWindow();
                }
            }
            PlayerTrackerService = container.Resolve<PlayerTrackerService>();
            logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
        }

        public class GithubAsset
        {
            public string browser_download_url { get; set; }
        }
        public class GithubVersionInfo
        {
            public List<GithubAsset> assets { get; set; }
            public string tag_name { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
        }

        private static string _Version = string.Empty;
        public static string Version
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_Version))
                {
                    return _Version;
                }
                var v = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#if BETA
                v = "Beta-" + v;
#endif
                _Version = v;
                return _Version;
            }
        }

        private void ToggleMenuButtons(bool value)
        {
            MapMenuItem.Enabled = value;
            SpellsMenuItem.Enabled = value;
            DpsMeterMenuItem.Enabled = value;
            MobInfoMenuItem.Enabled = value;
        }

        private void WhyThePig(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/nSrz8hAwxM",
                UseShellExecute = true
            });
        }

        private void Suggestions(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/issues",
                UseShellExecute = true
            });
        }

        private void LogParser_PlayerChangeEvent(object sender, LogParser.PlayerChangeEventArgs e)
        {
            if (FindEq.IsProject1999Folder(EQToolSettings.DefaultEqDirectory) && FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == true)
            {
                ToggleMenuButtons(true);
            }
        }

        private void ToggleWindow<T>(System.Windows.Forms.MenuItem m) where T : Window
        {
            var w = WindowList.FirstOrDefault(a => a.GetType() == typeof(T));
            m.Checked = !m.Checked;
            if (m.Checked)
            {
                if (w != null)
                {
                    _ = w.Focus();
                }
                else
                {
                    w?.Close();
                    w = container.Resolve<T>();
                    WindowList.Add(w);
                    w.Closed += (se, ee) =>
                    {
                        m.Checked = false;
                        _ = WindowList.Remove(w);
                    };
                    w.Show();
                }
            }
            else
            {
                w?.Close();
                _ = WindowList.Remove(w);
            }
        }

        private void OpenWindow<T>(System.Windows.Forms.MenuItem m) where T : Window
        {
            var w = WindowList.FirstOrDefault(a => a.GetType() == typeof(T));
            if (w != null)
            {
                _ = w.Focus();
            }
            else
            {
                m.Checked = true;
                w?.Close();
                w = container.Resolve<T>();
                WindowList.Add(w);
                w.Closed += (se, ee) =>
                {
                    m.Checked = false;
                    _ = WindowList.Remove(w);
                };
                w.Show();
            }
        }

        public void ToggleMapWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<MappingWindow>(s);
        }

        public void ToggleDPSWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<DPSMeter>(s);
        }

        public void ToggleMobInfoWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<MobInfo>(s);
        }

        public void ToggleSettingsWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<Settings>(s);
        }
        public void ToggleSpellsWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<SpellWindow>(s);
        }

        public void OpenDPSWindow()
        {
            OpenWindow<DPSMeter>(DpsMeterMenuItem);
        }

        public void OpenMapWindow()
        {
            OpenWindow<MappingWindow>(MapMenuItem);
        }

        public void OpenMobInfoWindow()
        {
            OpenWindow<MobInfo>(MobInfoMenuItem);
        }

        public void OpenSettingsWindow()
        {
            OpenWindow<Settings>(SettingsMenuItem);
        }

        public void OpenSpellsWindow()
        {
            OpenWindow<SpellWindow>(SpellsMenuItem);
        }

        private void OnExit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        internal static void ApplyAlwaysOnTop()
        {
            foreach (var item in WindowList)
            {
                if (item is DPSMeter w)
                {
                    w.Topmost = EQTool.Properties.Settings.Default.GlobalDpsWindowAlwaysOnTop;
                }
                else if (item is MappingWindow w1)
                {
                    w1.Topmost = EQTool.Properties.Settings.Default.GlobalMapWindowAlwaysOnTop;
                }
                else if (item is MobInfo w2)
                {
                    w2.Topmost = EQTool.Properties.Settings.Default.GlobalMobWindowAlwaysOnTop;
                }
                else if (item is SpellWindow w3)
                {
                    w3.Topmost = EQTool.Properties.Settings.Default.GlobalTriggerWindowAlwaysOnTop;
                }
            }
        }
    }
}
