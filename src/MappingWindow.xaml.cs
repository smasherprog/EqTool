using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static EQTool.ViewModels.MapViewModel;

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
            Topmost = true;
            InitializeComponent();
            if (settings.MapWindowState != null && WindowBounds.isPointVisibleOnAScreen(settings.MapWindowState.WindowRect))
            {
                Left = settings.MapWindowState.WindowRect.Left;
                Top = settings.MapWindowState.WindowRect.Top;
                Height = settings.MapWindowState.WindowRect.Height;
                Width = settings.MapWindowState.WindowRect.Width;
                WindowState = settings.MapWindowState.State;
            }
            if (settings.MapWindowState != null)
            {
                settings.MapWindowState.Closed = false;
            }
            App.ThemeChangedEvent += App_ThemeChangedEvent;
            _ = mapViewModel.LoadDefaultMap(Map);
            Map.Reset();
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.logParser.PlayerZonedEvent += LogParser_PlayerZonedEvent;
            SaveState();
            SizeChanged += Window_SizeChanged;
            StateChanged += Window_StateChanged;
            LocationChanged += Window_LocationChanged;
        }

        private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
        {
            mapViewModel.UpdateLocation(e.Location, Map);
        }

        private void LogParser_PlayerZonedEvent(object sender, LogParser.PlayerZonedEventArgs e)
        {
            var matched = ZoneParser.TranslateToMapName(e.Zone);
            if (mapViewModel.LoadMap(matched, Map))
            {
                Map.Reset();
            }
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

        private void SaveState()
        {
            if (settings.MapWindowState == null)
            {
                settings.MapWindowState = new Models.WindowState();
            }
            settings.MapWindowState.WindowRect = new Rect
            {
                X = Left,
                Y = Top,
                Height = Height,
                Width = Width
            };
            settings.MapWindowState.State = WindowState;
            toolSettingsLoad.Save(settings);
        }

        private void App_ThemeChangedEvent(object sender, App.ThemeChangeEventArgs e)
        {
            foreach (var item in Map.Children)
            {
                if (item is Line l)
                {
                    var c = l.Tag as Mapdata;
                    l.Stroke = new SolidColorBrush(e.Theme == Themes.Light ? c.MapColor.LightColor : c.MapColor.DarkColor);
                }
                else if (item is TextBlock t)
                {

                    var c = t.Tag as Mapdata;
                    t.Foreground = new SolidColorBrush(e.Theme == Themes.Light ? c.MapColor.LightColor : c.MapColor.DarkColor);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.ThemeChangedEvent -= App_ThemeChangedEvent;
            logParser.PlayerLocationEvent -= LogParser_PlayerLocationEvent;
            logParser.PlayerZonedEvent -= LogParser_PlayerZonedEvent;
            base.OnClosing(e);
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (settings.MapWindowState == null)
            {
                settings.MapWindowState = new Models.WindowState();
            }
            settings.MapWindowState.Closed = true;
            SaveState();
            Close();
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
    }
}
