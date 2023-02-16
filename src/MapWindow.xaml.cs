using EQTool.Services;
using EQTool.Services.Map;
using EQTool.ViewModels;
using HelixToolkit.Wpf;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using static EQTool.ViewModels.MapViewModel;

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
            viewport3d.Camera.Position = new Point3D(0, 1, 0);
            viewport3d.Camera.LookDirection = new Vector3D(0, 1, 0);
            viewport3d.Camera.UpDirection = new Vector3D(0, 1, 0);
            var camera = this.mapViewModel.LoadDefaultMap();
            if (camera != null)
            {
                viewport3d.Camera.Position = camera.Position;
                viewport3d.Camera.LookDirection = camera.LookDirection;
            }
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
            viewport3d.PanGesture = new MouseGesture(MouseAction.LeftClick);
            viewport3d.PanGesture2 = null;
            var debugging = false;
#if DEBUG
            debugging = true;
#endif
            if (debugging)
            {
                viewport3d.IsPanEnabled = false;
                viewport3d.ShowFrameRate = true;
                viewport3d.ShowCameraInfo = true;
            }
            else
            {
                viewport3d.IsPanEnabled = false;
                viewport3d.ShowFrameRate = false;
                viewport3d.ShowCameraInfo = false;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            logParser.LineReadEvent -= LogParser_LineReadEvent;
            base.OnClosing(e);
        }

        private void UITimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mapViewModel.Update();
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var pos = locationParser.Match(e.Line);
            CameraDetail camera;
            if (pos.HasValue)
            {
                camera = mapViewModel.UpdateLocation(pos.Value, new MapViewModel.CameraDetail
                {
                    Position = viewport3d.Camera.Position,
                    LookDirection = viewport3d.Camera.LookDirection
                });
            }
            else
            {
                var matched = zoneParser.Match(e.Line);
                matched = zoneParser.TranslateToMapName(matched);
                camera = mapViewModel.LoadMap(matched);
            }
            if (camera != null)
            {
                viewport3d.Camera.Position = camera.Position;
                viewport3d.Camera.LookDirection = camera.LookDirection;
            }
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

        private void viewport3d_CameraChanged(object sender, RoutedEventArgs e)
        {
            mapViewModel.UpdatePlayerVisual(viewport3d.Camera.Position);
        }
    }
}
