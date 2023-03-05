using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

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

        private EQToolSettings EQToolSettings => container.Resolve<EQToolSettings>();
        public static List<Window> WindowList = new List<Window>();

        private static Themes _Theme = Themes.Light;

        public static Themes Theme
        {
            get => _Theme;
            set
            {
                _Theme = value;
                EQTool.Properties.Settings.Default.ColorMode = value.ToString();
                ThemeChangedEvent?.Invoke(null, new ThemeChangeEventArgs { Theme = value });
            }
        }
        public class ThemeChangeEventArgs : EventArgs
        {
            public Themes Theme { get; set; }
        }

        public static event EventHandler<ThemeChangeEventArgs> ThemeChangedEvent;

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

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (!WaitForEQToolToStop())
            {
                MessageBox.Show("Another EQTool is currently running. You must shut that one down first!", "Multiple EQTools running!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }
            var debugging = false;
#if DEBUG
            debugging = true;
#endif
            if (!debugging)
            {
                AppCenter.Start("9be42804-8d4f-4431-9120-06f3a0370c4c", typeof(Analytics), typeof(Crashes));
            }

            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            var updateservice = new UpdateService();
            var did_update = UpdateService.UpdateStatus.NoUpdateApplied;
            try
            {
                did_update = updateservice.ApplyUpdate(e.Args.FirstOrDefault());
            }
            catch (Exception ex)
            {
                File.AppendAllText("Errors.txt", ex.ToString());
                MessageBox.Show(ex.Message);
                App.Current.Shutdown();
                return;
            }

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

#if Beta || DEBUG
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
                App.Theme = EQToolSettings.Theme;
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

        public string Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#if Beta
                v = "Beta-" + v;
#endif
                return v;
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
                OpenSpellsWindow();
                OpenDPSWindow();
                OpenMapWindow();
                OpenMobInfoWindow();
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
    }
}
