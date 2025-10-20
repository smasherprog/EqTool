using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using EQToolShared.Extensions;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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
        private System.Windows.Forms.MenuItem ClickThroughItem;
        private System.Windows.Forms.MenuItem OverlayMenuItem;
        private System.Windows.Forms.MenuItem SettingsMenuItem;
        private System.Windows.Forms.MenuItem MobInfoMenuItem;

        private EQToolSettings _EQToolSettings;
#if DEBUG
        private readonly bool IsDebug = true;
#else
        private readonly bool IsDebug = false;
#endif

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
            if (IsDebug)
            {
                return true;
            }
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
            if (counter > 1)
            {
                Thread.Sleep(1000);
            }
            return true;
        }

        public void CheckForUpdates(object sender, EventArgs e)
        {
            UpdateService.CheckForUpdates(Version, VersionType, container, false);
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
            Task.Factory.StartNew(() =>
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
            });
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
            var testFilePath = Paths.InExecutableDirectory("test.json");
            try
            {
                File.Delete(testFilePath);
            }
            catch { }
            try
            {
                File.WriteAllText(testFilePath, "test");
            }
            catch (UnauthorizedAccessException)
            {
                _ = MessageBox.Show("Pigparse is running from a directory where it does not have permission to save settings. Please, move it to a folder where it can write!", "Pigparse Permissions!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            try
            {
                File.Delete(testFilePath);
            }
            catch { }
            var cwd = Paths.ExecutableDirectory();
            if (cwd.ToLower().Contains("program files"))
            {
                _ = MessageBox.Show("Pigparse is running from a directory where it does not have permission to save settings. Please, move it to a folder where it can write!", "Pigparse Permissions!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
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
                _ = MessageBox.Show("Another Pigparse is currently running. You must shut that one down first!", "Multiple Pigparse running!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }
            try
            {
                var curr = Paths.ExecutableDirectory();
                var path = Paths.Combine(curr, "eqgame.exe");
                if (File.Exists(path))
                {
                    _ = MessageBox.Show("Pigparse does not support running from in the EQ directory. Please move the pigparse and try again", "Pigparse Invalid Folder!", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.Current.Shutdown();
                    return;
                }
            }
            catch { }
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            try
            {
                container = DI.Init();
            }
            catch (Exception ex)
            {
                LogUnhandledException(ex, "DI.Init", null);
            }

            var did_update = UpdateService.ApplyUpdate(e.Args.FirstOrDefault());
            if (did_update == UpdateService.UpdateStatus.UpdatesApplied)
            {
                return;
            }
            else if (did_update == UpdateService.UpdateStatus.NoUpdateApplied)
            {
                UpdateService.CheckForUpdates(Version, VersionType, container, true);
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
            container.Resolve<LoggingService>().Log(string.Empty, EventType.StartUp, null);
            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", ToggleSettingsWindow);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Triggers", ToggleSpellsWindow);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", ToggleMapWindow);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", ToggleDPSWindow);
            ClickThroughItem = new System.Windows.Forms.MenuItem("Click-Through", ToggleClickThrough);
            OverlayMenuItem = new System.Windows.Forms.MenuItem("Overlay", ToggleOverlayWindow);
            MobInfoMenuItem = new System.Windows.Forms.MenuItem("Mob Info", ToggleMobInfoWindow);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Pigparse Discord", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Check for Update", CheckForUpdates);

            var version = new System.Windows.Forms.MenuItem(Version)
            {
                Enabled = false
            };
            ToggleMenuButtons(false);
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                {
                    //GroupSuggestionsMenuItem,
                    whythepig,
                    ClickThroughItem,
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
            SystemTrayIcon.MouseClick += OnSystemTrayMouseClick;
            
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
                if (!EQToolSettings.SettingsWindowState.Closed)
                {
                    OpenSettingsWindow();
                }
            }
            
            ClickThroughItem.Checked = EQToolSettings.IsClickThroughMode;
            ApplyClickThrough(EQToolSettings.IsClickThroughMode);
            
            container.Resolve<SignalrPlayerHub>();
            container.Resolve<PlayerTrackerService>();
            container.Resolve<ZoneActivityTrackingService>();
            container.Resolve<IEnumerable<BaseHandler>>();
            container.Resolve<UIRunner>();
            container.Resolve<UpdateRunner>();

            App.Current.Resources["GlobalFontSize"] = (double)(EQToolSettings?.FontSize ?? 12);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleDPS", EQToolSettings.DpsWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleMap", EQToolSettings.MapWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleTrigger", EQToolSettings.SpellWindowState.Opacity.Value);
        }

        private void UpdateTrayIcon()
        {
#if BETA || DEBUG
            SystemTrayIcon.Icon = EQToolSettings.IsClickThroughMode ? EQTool.Properties.Resources.sickpic_t : EQTool.Properties.Resources.sickpic;
#else
            SystemTrayIcon.Icon = EQToolSettings.IsClickThroughMode ? EQTool.Properties.Resources.pig_t : EQTool.Properties.Resources.pig;
#endif
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
                var v = "P99";
#if BETA
                v= "Beta";
#elif LINUX
                v= "Linux";
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
            ToggleWindow<SettingManagement>(s);
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
            OpenWindow<SettingManagement>(SettingsMenuItem);
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
                    w.UpdateShowInTaskbar();
                }
                else if (item is MappingWindow w1)
                {
                    w1.Topmost = EQToolSettings.MapWindowState.AlwaysOnTop;
                    w1.UpdateShowInTaskbar();
                }
                else if (item is MobInfo w2)
                {
                    w2.Topmost = EQToolSettings.MobWindowState.AlwaysOnTop;
                    w2.UpdateShowInTaskbar();
                }
                else if (item is SpellWindow w3)
                {
                    w3.Topmost = EQToolSettings.SpellWindowState.AlwaysOnTop;
                    w3.UpdateShowInTaskbar();
                }
            }
        }

        private void OnSystemTrayMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            
            ApplyClickThrough(!EQToolSettings.IsClickThroughMode);
            ClickThroughItem.Checked = EQToolSettings.IsClickThroughMode;
        }
        
        private void ToggleClickThrough(object sender, EventArgs e)
        {
            ApplyClickThrough(!EQToolSettings.IsClickThroughMode);
            ClickThroughItem.Checked = EQToolSettings.IsClickThroughMode;
        }

        public void ApplyClickThrough(bool isClickThrough)
        {
            EQToolSettings.IsClickThroughMode = isClickThrough;
            UpdateTrayIcon();

            foreach (var item in WindowList)
            {
                if (item is DPSMeter w)
                {
                    w.SetClickThrough(isClickThrough && EQToolSettings.DpsWindowState.ClickThroughAllowed);
                }
                else if (item is MappingWindow w1)
                {
                    w1.SetClickThrough(isClickThrough && EQToolSettings.MapWindowState.ClickThroughAllowed);
                }
                else if (item is MobInfo w2)
                {
                    w2.SetClickThrough(isClickThrough && EQToolSettings.MobWindowState.ClickThroughAllowed);
                }
                else if (item is SpellWindow w3)
                {
                    w3.SetClickThrough(isClickThrough && EQToolSettings.SpellWindowState.ClickThroughAllowed);
                }
            }
        }
    }
}
