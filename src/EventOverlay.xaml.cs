using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for EventOverlay.xaml
    /// </summary>
    public partial class EventOverlay : Window
    {
        private readonly System.Timers.Timer UITimer;
        private readonly LogParser logParser;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private DateTime LastWindowInteraction = DateTime.UtcNow;
        public EventOverlay(LogParser logParser, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.settings = settings;
            this.logParser = logParser;
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.OverlayWindowState, this);
            this.Topmost = true;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            this.toolSettingsLoad = toolSettingsLoad;
            SizeChanged += Window_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += Window_LocationChanged;
            logParser.EnrageEvent += LogParser_EnrageEvent;
            settings.OverlayWindowState.Closed = false;
            SaveState();
        }

        private void LogParser_EnrageEvent(object sender, EnrageParser.EnrageEvent e)
        {
            var overlay = this.activePlayer?.Player?.EnrageOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            CenterText.Text = e.NpcName + " ENRAGED";
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 10);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "ENGRAGE OFF";
                });
                System.Threading.Thread.Sleep(1000 * 3);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                });
            });
        }

        private void SaveState()
        {
            WindowExtensions.SaveWindowState(settings.OverlayWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            LastWindowInteraction = DateTime.UtcNow;
            DragMove();
        }

        private void PollUI(object sender, EventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            SizeChanged -= Window_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= Window_LocationChanged;
            logParser.EnrageEvent -= LogParser_EnrageEvent;
            SaveState();
            base.OnClosing(e);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            WindowResizeChrome.ResizeBorderThickness = new Thickness(8);
            WindowBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            LastWindowInteraction = DateTime.UtcNow;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (Math.Abs((DateTime.UtcNow - LastWindowInteraction).TotalSeconds) < 10)
                {
                    System.Threading.Thread.Sleep(1000 * 1);
                }
                this.appDispatcher.DispatchUI(() =>
                {
                    WindowResizeChrome.ResizeBorderThickness = new Thickness(0);
                    WindowBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                });
            });
        }
    }
}
