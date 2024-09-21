using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.P99LoginMiddlemand;
using EQTool.UI;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EQTool
{
    public partial class App : Application
    {
        public static HttpClient httpclient = new HttpClient();

        private Autofac.IContainer container;
        private System.Windows.Forms.NotifyIcon SystemTrayIcon;

        private System.Windows.Forms.MenuItem MapMenuItem;
        private System.Windows.Forms.MenuItem SpellsMenuItem;
        private System.Windows.Forms.MenuItem DpsMeterMenuItem;
        private System.Windows.Forms.MenuItem OverlayMenuItem;
        private System.Windows.Forms.MenuItem SettingsMenuItem;
        private System.Windows.Forms.MenuItem GroupSuggestionsMenuItem;
        private System.Windows.Forms.MenuItem MobInfoMenuItem;
        private LogParser logParser => container.Resolve<LogParser>();
        private LogEvents logEvents => container.Resolve<LogEvents>();
        private System.Timers.Timer UITimer;
        private PlayerTrackerService PlayerTrackerService;
        private ZoneActivityTrackingService ZoneActivityTrackingService;
        private ISignalrPlayerHub signalrPlayerHub;
        private AudioService audioService;

        private EQToolSettings _EQToolSettings;

        private EQToolSettings EQToolSettings
        {
            get
            {
                if (_EQToolSettings == null)
                {
                    _EQToolSettings = container.Resolve<EQToolSettings>();
                }
                return _EQToolSettings;
            }
        }

        public static List<BaseSaveStateWindow> WindowList = new List<BaseSaveStateWindow>();
        private const string programName = "eqtool";
        private bool WaitForEQToolToStop()
        {
#if DEBUG
            return true;
#endif
            var counter = 0;
            int count;
            do
            {
                count = Process.GetProcessesByName(programName).Count();
                if (counter++ > 6)
                {
                    return false;
                }
                Debug.WriteLine($"Waiting for {programName} {count} on counter {counter}");
                Thread.Sleep(3000);
            }
            while (count != 1);
            return true;
        }

        public void CheckForUpdates(object sender, EventArgs e)
        {
            new UpdateService().CheckForUpdates(Version, VersionType, container);
        }

        public class ExceptionRequest
        {
            public string Version { get; set; }
            public string Message { get; set; }
            public EventType EventType { get; set; }
            public BuildType BuildType { get; set; }
            public Servers? Server { get; set; }
        }

        public static void LogUnhandledException(Exception exception, string source, Servers? server)
        {
            var build = BuildType.Release;
#if TEST
            build =  BuildType.Test;
#elif DEBUG
            build = BuildType.Debug;
#elif BETA
            build = BuildType.Beta;
#elif QUARM
            build = BuildType.Quarm;
#endif
            try
            {
                var msg = new ExceptionRequest
                {
                    Version = Version,
                    Message = $"Unhandled exception ({source}) {exception}",
                    EventType = EventType.Error,
                    BuildType = build,
                    Server = server
                };
                if (msg.Message.Contains("Server timeout (30000.00ms) elapsed without receiving a message from the server.") ||
                    msg.Message.Contains("The 'InvokeCoreAsync' method cannot be called") ||
                     msg.Message.Contains("The remote party closed the WebSocket connection") ||
                     msg.Message.Contains("An internal WebSocket error occurred.")
                    )
                {
                    return;
                }
                var msagasjson = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                var content = new StringContent(msagasjson, Encoding.UTF8, "application/json");
                var result = httpclient.PostAsync("https://pigparse.azurewebsites.net/api/eqtool/exception", content).Result;
            }
            catch { }
        }

        private void SetupExceptionHandling()
        {

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var server = container?.Resolve<ActivePlayer>()?.Player?.Server;
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException", server);
            };

            DispatcherUnhandledException += (s, e) =>
            {
                var server = container?.Resolve<ActivePlayer>()?.Player?.Server;
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException", server);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                var server = container?.Resolve<ActivePlayer>()?.Player?.Server;
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException", server);
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
            ServicePointManager.ServerCertificateValidationCallback += (sender1, cert, chain, sslPolicyErrors) => true;
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
            try
            {
                var curr = Directory.GetCurrentDirectory();
                var path = Path.Combine(curr, "eqgame.exe");
                if (File.Exists(path))
                {
                    MessageBox.Show("Pigparse does not support running from in the EQ directory. Please move the pigparse and try again", "Pigparse Invalid Folder!", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Current.Shutdown();
                    return;
                }
            }
            catch { }
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
                updateservice.CheckForUpdates(Version, VersionType);
#endif
            }

            try
            {
                InitStuff();
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex, "InitStuff", null);
                Thread.Sleep(1000 * 20);/// Sleep for 20 seconds here this will hopfully allow the update to occur and fix any problems
            }
        }

        private void InitStuff()
        {
            container = DI.Init();
            UITimer = new System.Timers.Timer(1000 * 60);
#if !DEBUG
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
#endif
            container.Resolve<LoggingService>().Log(string.Empty, EventType.StartUp, null);
            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", ToggleSettingsWindow);
            var standardgroup = new System.Windows.Forms.MenuItem("Standard Groups", CreateStandardGroup);
            var hotclericsamegroup = new System.Windows.Forms.MenuItem("HOT Clerics Same Group", CreateHOTClericsSameGroup);
            var hotclericsparsegroup = new System.Windows.Forms.MenuItem("HOT Clerics Sparse Group", CreateHOTClericsSparseGroup);
            GroupSuggestionsMenuItem = new System.Windows.Forms.MenuItem("Group Suggestions", new System.Windows.Forms.MenuItem[] { standardgroup, hotclericsamegroup, hotclericsparsegroup });
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Triggers", ToggleSpellsWindow);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", ToggleMapWindow);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", ToggleDPSWindow);
            OverlayMenuItem = new System.Windows.Forms.MenuItem("Overlay", ToggleOverlayWindow);
            MobInfoMenuItem = new System.Windows.Forms.MenuItem("Mob Info", ToggleMobInfoWindow);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Pigparse Discord", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Check for Update", CheckForUpdates);
            var versionstring = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var logo = EQTool.Properties.Resources.pig;

#if BETA || DEBUG
            logo = EQTool.Properties.Resources.sickpic;
#endif

            var version = new System.Windows.Forms.MenuItem(Version)
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
                    //GroupSuggestionsMenuItem,
                    whythepig,
                    OverlayMenuItem,
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
            var hasvalideqdir = FindEq.IsValidEqFolder(EQToolSettings.DefaultEqDirectory);
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
                if (!EQToolSettings.OverlayWindowState.Closed)
                {
                    OpenOverLayWindow();
                }
            }
            signalrPlayerHub = container.Resolve<ISignalrPlayerHub>();

            PlayerTrackerService = container.Resolve<PlayerTrackerService>();
            ZoneActivityTrackingService = container.Resolve<ZoneActivityTrackingService>();
            audioService = container.Resolve<AudioService>();
            logEvents.QuakeEvent += LogParser_QuakeEvent;
            App.Current.Resources["GlobalFontSize"] = (double)(EQToolSettings?.FontSize ?? 12);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleDPS", EQToolSettings.DpsWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleMap", EQToolSettings.MapWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleTrigger", EQToolSettings.SpellWindowState.Opacity.Value);

            if (EQToolSettings.LoginMiddleMand)
            {
                var loginmiddlemand = container.Resolve<LoginMiddlemand>();
                if (loginmiddlemand.IsConfiguredCorrectly())
                {
                    loginmiddlemand.StartListening();
                }
                else
                {
                    EQToolSettings.LoginMiddleMand = false;
                }
            }
        }

        public void UpdateBackgroundOpacity(string name, double opacity)
        {
            var newcolor = (SolidColorBrush)new BrushConverter().ConvertFrom("#1a1919");
            newcolor.Opacity = opacity;
            var style = new System.Windows.Style { TargetType = typeof(Window) };
            style.Setters.Add(new Setter(Window.BackgroundProperty, newcolor));
            style.Setters.Add(new Setter(Window.FontSizeProperty, (double)EQToolSettings.FontSize.Value));
            App.Current.Resources[name] = style;
        }

        private void LogParser_QuakeEvent(object sender, QuakeEvent e)
        {
            container.Resolve<PigParseApi>().SendQuake();
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public int dwTime;
        }
        public static TimeSpan GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            return GetLastInputInfo(ref lastInPut)
                ? TimeSpan.FromMilliseconds(Environment.TickCount - lastInPut.dwTime)
                : TimeSpan.FromMinutes(20);
        }

        private bool updatecalled = false;
        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var dispatcher = container.Resolve<IAppDispatcher>();
            dispatcher.DispatchUI(() =>
            {
                if (updatecalled)
                {
                    return;
                }
                updatecalled = true;
                try
                {
                    var idletime = GetIdleTime();
                    var spellstuff = container.Resolve<SpellWindowViewModel>();
                    var logParser = container.Resolve<LogParser>();
                    if (spellstuff != null)
                    {
                        if (spellstuff.SpellList.Count() < 2 && (DateTime.UtcNow - logParser.LastYouActivity).TotalMinutes > 10 && idletime.TotalMinutes > 10)
                        {
                            new UpdateService().CheckForUpdates(Version, VersionType, container);
                        }
                    }
                    else if ((DateTime.UtcNow - logParser.LastYouActivity).TotalMinutes > 10 && idletime.TotalMinutes > 10)
                    {
                        new UpdateService().CheckForUpdates(Version, VersionType, container);
                    }
                }
                finally
                {
                    updatecalled = false;
                }
            });
        }

        public class GithubAsset
        {
            public string browser_download_url { get; set; }
        }

        public class GithubVersionInfo
        {
            public List<GithubAsset> assets { get; set; }
            public string name { get; set; }
            public string tag_name { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
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
                var v = VersionType + Assembly.GetExecutingAssembly().GetName().Version.ToString();
                _Version = v;
                return _Version;
            }
        }

        public static string VersionType
        {
            get
            {
                var v = string.Empty;
#if BETA
                v= "Beta";
#elif LINUX
                v= "Linux";
#elif QUARM
                v= "Quarm";
#else
                v = "P99";
#endif
                return v;
            }
        }


        public void ToggleMenuButtons(bool value)
        {
            MapMenuItem.Enabled = value;
            SpellsMenuItem.Enabled = value;
            DpsMeterMenuItem.Enabled = value;
            MobInfoMenuItem.Enabled = value;
            GroupSuggestionsMenuItem.Enabled = value;
        }

        private void CreateStandardGroup(object sender, EventArgs e)
        {
            CreateGroup(GroupOptimization.Standard);
        }

        private void CreateHOTClericsSparseGroup(object sender, EventArgs e)
        {
            CreateGroup(GroupOptimization.HOT_Cleric_SparseGroup);
        }

        private void CreateHOTClericsSameGroup(object sender, EventArgs e)
        {
            CreateGroup(GroupOptimization.HOT_Cleric_SameGroup);
        }

        private void CreateGroup(GroupOptimization grp)
        {
            var grpstring = new List<string>();
            var groups = PlayerTrackerService.CreateGroups(grp);
            var groupindex = 1;
            foreach (var group in groups)
            {
                var str = $"/gu Group {groupindex++} ";
                foreach (var player in group.Players)
                {
                    str += player.Name + ",";
                }
                grpstring.Add(str);
            }
            if (grpstring.Any())
            {
                System.Windows.Forms.Clipboard.SetText(string.Join("\r\n", grpstring));
            }
            else
            {
                System.Windows.Forms.Clipboard.SetText("You must /who in the zone before group suggestions can be made!");
            }
        }

        private void WhyThePig(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/rkU8ewzWWk",
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

        private void ToggleWindow<T>(System.Windows.Forms.MenuItem m) where T : BaseSaveStateWindow
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
                w?.CloseWindow();
                _ = WindowList.Remove(w);
            }
        }

        private void OpenWindow<T>(System.Windows.Forms.MenuItem m) where T : BaseSaveStateWindow
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

        public void ToggleOverlayWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            ToggleWindow<EventOverlay>(s);
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

        public void OpenOverLayWindow()
        {
            OpenWindow<EventOverlay>(OverlayMenuItem);
        }

        public void OpenSettingsWindow()
        {
            OpenWindow<Settings>(SettingsMenuItem);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, System.Windows.Forms.ToolTipIcon tipIcon)
        {
            SystemTrayIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        public void OpenSpellsWindow()
        {
            OpenWindow<SpellWindow>(SpellsMenuItem);
        }

        private void OnExit(object sender, EventArgs e)
        {
            var loginmiddlemand = container.Resolve<LoginMiddlemand>();
            loginmiddlemand.StopListening();
            System.Windows.Application.Current.Shutdown();
        }

        public void ApplyAlwaysOnTop()
        {
            foreach (var item in WindowList)
            {
                if (item is DPSMeter w)
                {
                    w.Topmost = EQToolSettings.DpsWindowState.AlwaysOnTop;
                }
                else if (item is MappingWindow w1)
                {
                    w1.Topmost = EQToolSettings.MapWindowState.AlwaysOnTop;
                }
                else if (item is MobInfo w2)
                {
                    w2.Topmost = EQToolSettings.MobWindowState.AlwaysOnTop;
                }
                else if (item is SpellWindow w3)
                {
                    w3.Topmost = EQToolSettings.SpellWindowState.AlwaysOnTop;
                }
            }
        }
    }
}
