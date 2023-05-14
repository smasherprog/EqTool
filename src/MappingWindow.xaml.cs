using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
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

        public MappingWindow(MapViewModel mapViewModel, LogParser logParser, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad)
        {
            this.settings = settings;
            this.toolSettingsLoad = toolSettingsLoad;
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
            settings.MapWindowState.Closed = false;
            SaveState();
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
            var textcolor = new SolidColorBrush(App.Theme == Themes.Light ? System.Windows.Media.Color.FromRgb(0, 0, 0) : System.Windows.Media.Color.FromRgb(255, 255, 255));
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
                    t.Foreground = textcolor;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.ThemeChangedEvent -= App_ThemeChangedEvent;
            logParser.PlayerLocationEvent -= LogParser_PlayerLocationEvent;
            logParser.PlayerZonedEvent -= LogParser_PlayerZonedEvent;
            logParser.PlayerChangeEvent -= LogParser_PlayerChangeEvent;
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
    }
}
