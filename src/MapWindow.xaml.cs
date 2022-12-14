using EQTool.Services;
using EQTool.Services.Map;
using EQTool.ViewModels;
using HelixToolkit.Wpf;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private readonly Timer UITimer;
        private readonly LogParser logParser;
        private readonly MapViewModel mapViewModel;
        private readonly LocationParser locationParser;
        private readonly ZoneParser zoneParser;

        public MapWindow(ZoneParser zoneParser, MapViewModel mapViewModel, LocationParser locationParser, LogParser logParser)
        {
            this.zoneParser = zoneParser;
            this.locationParser = locationParser;
            this.logParser = logParser;
            DataContext = this.mapViewModel = mapViewModel;
            Topmost = true;
            InitializeComponent();
            this.mapViewModel.LoadDefaultMap();
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
            viewport3d.PanGesture = new MouseGesture(MouseAction.LeftClick);
            viewport3d.PanGesture2 = null;

        }

        private void UITimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mapViewModel.Update();
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var pos = locationParser.Match(e.Line);
            if (pos.HasValue)
            {
                mapViewModel.UpdateLocation(pos.Value);
            }
            else
            {
                var matched = zoneParser.Match(e.Line);
                _ = mapViewModel.LoadMap(matched);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            logParser.LineReadEvent += LogParser_LineReadEvent;
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
            Close();
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

        private void viewport3d_MouseMove(object sender, MouseEventArgs e)
        {
            var vp = viewport3d;
            var mpt = Mouse.GetPosition(vp);
            var hitTests = viewport3d.Viewport.FindHits(mpt);

            foreach (var hit in hitTests.OrderBy(a => a.Distance).Take(1))
            {
                mapViewModel.MouseWorldCoordinates = hit.RayHit.PointHit;
            }
        }
    }
}
