using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static EQTool.Services.MapLoad;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class MappingWindow : Window
    {
        private readonly LogParser logParser;
        private readonly MapViewModel mapViewModel;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly IAppDispatcher appDispatcher;
        private readonly System.Timers.Timer UITimer;
        private bool AutomaticallyAddTimerOnDeath = false;

        public MappingWindow(MapViewModel mapViewModel, LogParser logParser, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, IAppDispatcher appDispatcher)
        {
            this.settings = settings;
            this.toolSettingsLoad = toolSettingsLoad;
            this.appDispatcher = appDispatcher;
            this.logParser = logParser;
            DataContext = this.mapViewModel = mapViewModel;
            InitializeComponent();

            WindowExtensions.AdjustWindow(settings.MapWindowState, this);
            Topmost = Properties.Settings.Default.GlobalMapWindowAlwaysOnTop;
            App.ThemeChangedEvent += App_ThemeChangedEvent;
            _ = mapViewModel.LoadDefaultMap(Map);
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.logParser.PlayerZonedEvent += LogParser_PlayerZonedEvent;
            this.logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            SizeChanged += Window_SizeChanged;
            StateChanged += Window_StateChanged;
            LocationChanged += Window_LocationChanged;
            KeyDown += PanAndZoomCanvas_KeyDown;
            settings.MapWindowState.Closed = false;
            SaveState();
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
        }

        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
        {
            if (AutomaticallyAddTimerOnDeath)
            {
                var timer = Map.ZoneRespawnTime;
                var mw = Map.AddTimer(timer, e.Name);
                mapViewModel.MoveToPlayerLocation(mw, Map.Transform);
            };
        }

        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            appDispatcher.DispatchUI(() => Map.UpdateTimerWidgest());
        }

        private void PanAndZoomCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            var scale = (int)MathHelper.ChangeRange(Math.Max(mapViewModel.AABB.MaxWidth, mapViewModel.AABB.MaxHeight), 500, 35000, 60, 300);
            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    Map.MoveMap(scale, 0);
                    break;
                case Key.Right:
                case Key.D:
                    Map.MoveMap(-scale, 0);
                    break;
                case Key.Up:
                case Key.W:
                    Map.MoveMap(0, scale);
                    break;
                case Key.Down:
                case Key.S:
                    Map.MoveMap(0, -scale);
                    break;
                default:
                    return;

            }
        }

        private void LogParser_PlayerChangeEvent(object sender, LogParser.PlayerChangeEventArgs e)
        {
            _ = mapViewModel.LoadDefaultMap(Map);
        }

        private void LogParser_PlayerZonedEvent(object sender, LogParser.PlayerZonedEventArgs e)
        {
            _ = mapViewModel.LoadMap(e.Zone, Map);
        }

        private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
        {
            mapViewModel.UpdateLocation(e.Location, Map);
        }

        private void App_ThemeChangedEvent(object sender, App.ThemeChangeEventArgs e)
        {
            foreach (var item in Map.Children)
            {
                if (item is Line l)
                {
                    var c = l.Tag as MapLine;
                    l.Stroke = new SolidColorBrush(e.Theme == Themes.Light ? c.ThemeColors.LightColor : c.ThemeColors.DarkColor);
                }
                else if (item is TextBlock t)
                {
                    _ = t.Tag as MapLabel;
                    t.Foreground = new SolidColorBrush(App.Theme == Themes.Light ? System.Windows.Media.Color.FromRgb(0, 0, 0) : System.Windows.Media.Color.FromRgb(255, 255, 255));
                }
                else if (item is MapWidget mw)
                {
                    _ = mw.Tag as MapLabel;
                    var textlabel = new SolidColorBrush(App.Theme == Themes.Light ? System.Windows.Media.Color.FromRgb(0, 0, 0) : System.Windows.Media.Color.FromRgb(255, 255, 255));
                    var forgregroundlabel = new SolidColorBrush(App.Theme == Themes.Light ? System.Windows.Media.Color.FromRgb(255, 255, 255) : System.Windows.Media.Color.FromRgb(0, 0, 0));
                    mw.SetTheme(textlabel, forgregroundlabel);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            App.ThemeChangedEvent -= App_ThemeChangedEvent;
            logParser.PlayerLocationEvent -= LogParser_PlayerLocationEvent;
            logParser.PlayerZonedEvent -= LogParser_PlayerZonedEvent;
            logParser.PlayerChangeEvent -= LogParser_PlayerChangeEvent;
            logParser.DeadEvent -= LogParser_DeadEvent;
            SizeChanged -= Window_SizeChanged;
            StateChanged -= Window_StateChanged;
            LocationChanged -= Window_LocationChanged;
            SaveState();
            base.OnClosing(e);
        }

        private void SaveState()
        {
            WindowExtensions.SaveWindowState(settings.MapWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            settings.MapWindowState.Closed = true;
            Close();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveState();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
        }

        private void openmobinfo(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMobInfoWindow();
        }

        private void opendps(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenDPSWindow();
        }

        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWindow();
        }
        private void openmap(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMapWindow();
        }

        private void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWindow();
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = Map.Transform.Inverse.Transform(e.GetPosition(sender as IInputElement));
            mapViewModel.MouseMove(mousePosition, sender, e);
        }

        private void autoaddtimer(object sender, RoutedEventArgs e)
        {
            AutomaticallyAddTimerOnDeath = !AutomaticallyAddTimerOnDeath;
            if (AutomaticallyAddTimerOnDeath)
            {
                TimerToggle.ToolTip = "Stop adding timer to npcs on death.";
                TimerToggle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
            }
            else
            {
                TimerToggle.ToolTip = "Add timer to npcs on death.";
                TimerToggle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
            }
        }
    }
}
