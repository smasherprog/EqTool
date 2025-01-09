using EQTool.Models;
using EQTool.Services.Handlers;
using EQTool.Services.P99LoginMiddlemand;
using EQTool.UI;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EQTool.Services
{
    public class AppRoot
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly LoggingService loggingService;
        private readonly UpdateService updateService;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQToolSettings toolSettings;
        private readonly SignalrPlayerHub signalrPlayerHub;
        private readonly LoginMiddlemand loginMiddlemand;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly ZoneActivityTrackingService zoneActivityTrackingService;
        private readonly WindowFactory windowFactory;

        private readonly System.Timers.Timer UpdateTimer;
        private readonly System.Timers.Timer UITimer;

        private bool updatecalled = false;

        public AppRoot(LogParser logParser,
            LogEvents logEvents,
            UpdateService updateService,
            IAppDispatcher appDispatcher,
            ActivePlayer activePlayer,
            SpellWindowViewModel spellWindowViewModel,
            LoggingService loggingService,
            EQToolSettings toolSettings,
            SignalrPlayerHub signalrPlayerHub,
            LoginMiddlemand loginMiddlemand,
            PlayerTrackerService playerTrackerService,
            ZoneActivityTrackingService zoneActivityTrackingService,
            IEnumerable<BaseHandler> handlers,
            WindowFactory windowFactory)
        {
            _ = handlers.ToList();
            this.logParser = logParser;
            this.logEvents = logEvents;
            this.updateService = updateService;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.spellWindowViewModel = spellWindowViewModel;
            this.loggingService = loggingService;
            this.toolSettings = toolSettings;
            this.signalrPlayerHub = signalrPlayerHub;
            this.loginMiddlemand = loginMiddlemand;
            this.zoneActivityTrackingService = zoneActivityTrackingService;
            this.playerTrackerService = playerTrackerService;
            this.windowFactory = windowFactory;



            UpdateTimer = new System.Timers.Timer(1000 * 60);
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Enabled = true;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
        }
        private void OnExit(object sender, EventArgs e)
        {
            loginMiddlemand.StopListening();
            System.Windows.Application.Current.Shutdown();
        }
        private void CheckForUpdates(object sender, EventArgs e)
        {
            updateService.CheckForUpdates(Version, VersionType, false);
        }

        public void Init()
        {
            loggingService.Log(string.Empty, EventType.StartUp, activePlayer?.Player?.Server);
            App.Current.Resources["GlobalFontSize"] = (double)(toolSettings.FontSize ?? 12);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleDPS", toolSettings.DpsWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleMap", toolSettings.MapWindowState.Opacity.Value);
            ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleTrigger", toolSettings.SpellWindowState.Opacity.Value);

            if (toolSettings.LoginMiddleMand)
            {
                if (loginMiddlemand.IsConfiguredCorrectly())
                {
                    loginMiddlemand.StartListening();
                }
                else
                {
                    toolSettings.LoginMiddleMand = false;
                }
            }

            var hasvalideqdir = FindEq.IsValidEqFolder(toolSettings.DefaultEqDirectory);
            if (!hasvalideqdir || FindEq.TryCheckLoggingEnabled(toolSettings.DefaultEqDirectory) == false)
            {
                if (!hasvalideqdir)
                {
                    toolSettings.DefaultEqDirectory = string.Empty;
                }
                OpenSettingsWindow();
            }
            else
            {
                ToggleMenuButtons(true);
                if (!toolSettings.SpellWindowState.Closed)
                {
                    OpenSpellsWindow();
                }
                if (!toolSettings.DpsWindowState.Closed)
                {
                    OpenDPSWindow();
                }
                if (!toolSettings.MapWindowState.Closed)
                {
                    OpenMapWindow();
                }
                if (!toolSettings.MobWindowState.Closed)
                {
                    OpenMobInfoWindow();
                }
                if (!toolSettings.OverlayWindowState.Closed)
                {
                    OpenOverLayWindow();
                }
                if (!toolSettings.SettingsWindowState.Closed)
                {
                    OpenSettingsWindow();
                }
            }
        }

        public void ApplyAlwaysOnTop()
        {
            foreach (var item in windowFactory.WindowList)
            {
                if (item is DPSMeter w)
                {
                    w.Topmost = toolSettings.DpsWindowState.AlwaysOnTop;
                }
                else if (item is MappingWindow w1)
                {
                    w1.Topmost = toolSettings.MapWindowState.AlwaysOnTop;
                }
                else if (item is MobInfo w2)
                {
                    w2.Topmost = toolSettings.MobWindowState.AlwaysOnTop;
                }
                else if (item is SpellWindow w3)
                {
                    w3.Topmost = toolSettings.SpellWindowState.AlwaysOnTop;
                }
            }
        }

        private void ToggleMenuButtons(bool value)
        {
            MapMenuItem.Enabled = value;
            SpellsMenuItem.Enabled = value;
            DpsMeterMenuItem.Enabled = value;
            MobInfoMenuItem.Enabled = value;
        }

        private DateTime? LastUIRun = null;
        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var dt_ms = 0.0;
            if (LastUIRun.HasValue)
            {
                dt_ms = (now - LastUIRun.Value).TotalMilliseconds;
            }

            LastUIRun = now;
            spellWindowViewModel.UpdateSpells(dt_ms);
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public int dwTime;
        }
        private static TimeSpan GetIdleTime()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            return GetLastInputInfo(ref lastInPut)
                ? TimeSpan.FromMilliseconds(Environment.TickCount - lastInPut.dwTime)
                : TimeSpan.FromMinutes(20);
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (updatecalled)
                {
                    return;
                }
                updatecalled = true;
                try
                {
                    var idletime = GetIdleTime();
                    if (spellWindowViewModel.SpellList.Count() < 2 && (DateTime.Now - logParser.LastYouActivity).TotalMinutes > 10 && idletime.TotalMinutes > 10)
                    {
                        updateService.CheckForUpdates(Version, VersionType, false);
                    }
                }
                finally
                {
                    updatecalled = false;
                }
            });
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

        private class ExceptionRequest
        {
            public string Version { get; set; }
            public string Message { get; set; }
            public EventType EventType { get; set; }
            public BuildType BuildType { get; set; }
            public Servers? Server { get; set; }
        }

        public void LogUnhandledException(Exception exception, string source, string version)
        {
            var server = activePlayer.Player?.Server;
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
                    Version = version,
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
                var result = App.httpclient.PostAsync("https://pigparse.azurewebsites.net/api/eqtool/exception", content).Result;
            }
            catch { }
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

        private void ToggleMapWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<MappingWindow>(s);
        }

        private void ToggleDPSWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<DPSMeter>(s);
        }

        private void ToggleOverlayWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<EventOverlay>(s);
        }

        private void ToggleMobInfoWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<MobInfo>(s);
        }

        private void ToggleSettingsWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<SettingManagement>(s);
        }

        private void ToggleSpellsWindow(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            windowFactory.ToggleWindow<SpellWindow>(s);
        }

        private void OpenDPSWindow()
        {
            windowFactory.OpenWindow<DPSMeter>(DpsMeterMenuItem);
        }

        private void OpenMapWindow()
        {
            windowFactory.OpenWindow<MappingWindow>(MapMenuItem);
        }

        private void OpenMobInfoWindow()
        {
            windowFactory.OpenWindow<MobInfo>(MobInfoMenuItem);
        }

        private void OpenOverLayWindow()
        {
            windowFactory.OpenWindow<EventOverlay>(OverlayMenuItem);
        }

        private void OpenSettingsWindow()
        {
            windowFactory.OpenWindow<SettingManagement>(SettingsMenuItem);
        }

        private void ShowBalloonTip(int timeout, string tipTitle, string tipText, System.Windows.Forms.ToolTipIcon tipIcon)
        {
            SystemTrayIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        private void OpenSpellsWindow()
        {
            windowFactory.OpenWindow<SpellWindow>(SpellsMenuItem);
        }
    }
}
