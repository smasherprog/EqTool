using EQTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EQTool
{
    public partial class PanAndZoomCanvas : Canvas
    {
        public MapViewModel mapViewModel;

        public PanAndZoomCanvas(MapViewModel mapViewModel)
        {
            InitializeComponent();
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(240, 30, 30, 30));
            DataContext = mapViewModel;
            MouseDown += PanAndZoomCanvas_MouseDown;
            MouseUp += PanAndZoomCanvas_MouseUp;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;
            TimeSpanControl.DefaultValue = TimeSpan.FromMinutes(72);
            TimeSpanControl.Value = TimeSpan.FromMinutes(72);
            TimeSpanControl.DisplayDefaultValueOnEmptyText = true;
        }

        private void AddTimer(object sender, RoutedEventArgs e)
        {
            if (TimeSpanControl.Value.HasValue)
            {
                _ = mapViewModel.AddTimer(TimeSpanControl.Value.Value, string.Empty);
                TimerMenu.IsOpen = false;
                mapViewModel.TimerMenu_Closed();
            }
        }

        private void DeleteTimer(object sender, RoutedEventArgs e)
        {
            mapViewModel.DeleteSelectedTimer();
        }

        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (e.Source is MapWidget)
                {
                    AddTimerMenuItem.Visibility = Visibility.Collapsed;
                    DeleteTimerMenuItem.Visibility = Visibility.Visible;
                }
                else
                {
                    AddTimerMenuItem.Visibility = Visibility.Visible;
                    DeleteTimerMenuItem.Visibility = Visibility.Collapsed;
                }
            }
            var mousePostion = e.GetPosition(this);
            mapViewModel.PanAndZoomCanvas_MouseDown(mousePostion, e);
        }

        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var mousePostion = e.GetPosition(this);
            mapViewModel.PanAndZoomCanvas_MouseUp(mousePostion);
        }

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePostion = e.GetPosition(this);
            mapViewModel.PanAndZoomCanvas_MouseMove(mousePostion, e);
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePostion = e.GetPosition(this);
            mapViewModel.PanAndZoomCanvas_MouseWheel(mousePostion, e.Delta);
        }
        private void TimerMenu_Closed(object sender, RoutedEventArgs e)
        {
            mapViewModel.TimerMenu_Closed();
        }

        private void TimerMenu_Opened(object sender, RoutedEventArgs e)
        {
            mapViewModel.TimerMenu_Opened();
            TimeSpanControl.Value = EQToolShared.Map.ZoneParser.ZoneInfoMap.TryGetValue(mapViewModel.ZoneName, out var zoneInfo) ? zoneInfo.RespawnTime : new TimeSpan(0, 6, 40);
        }
    }
}
