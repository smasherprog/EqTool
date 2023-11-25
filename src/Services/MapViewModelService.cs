using EQTool.Models;
using EQTool.Shapes;
using EQTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace EQTool.Services
{
    public class AddPlayerToCanvasData
    {
        public string Name { get; set; }
        public double? Trackingdistance { get; set; }
        public MapLoad.AABB AABB { get; set; }
        public Transform Transform { get; set; }
        public Canvas Canvas { get; set; }
    }
    public class UpdateLocationData
    {
        public Point3D MapOffset { get; set; }
        public PlayerLocationCircle PlayerLocationCircle { get; set; }
        public Point3D Newlocation { get; set; }
        public Point3D Oldlocation { get; set; }
        public double? Trackingdistance { get; set; }
        public float CurrentScaling { get; set; }
        public Transform Transform { get; set; }
    }
    public static class MapViewModelService
    {
        public static double ZoneLabelFontSize(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 14, 170); }
        public static double OtherLabelFontSize(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 6, 110); }
        public static double SmallFontSize(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 7, 50); }
        public static double PlayerEllipsesSize(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 10, 170); }
        public static double PlayerEllipsesThickness(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 3, 24); }
        public static double MapLinethickness(MapLoad.AABB aabb) { return MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 800, 35000, 2, 40); }
        private static double GetAngleBetweenPoints(Point3D pt1, Point3D pt2)
        {
            var dx = pt2.X - pt1.X;
            var dy = pt2.Y - pt1.Y;
            var deg = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (deg < 0) { deg += 360; }

            return deg;
        }

        public static void UpdateLocation(UpdateLocationData locationData)
        {
            var newdir = new Point3D(locationData.Newlocation.X, locationData.Newlocation.Y, 0) - new Point3D(locationData.Oldlocation.X, locationData.Oldlocation.Y, 0);
            newdir.Normalize();
            var angle = GetAngleBetweenPoints(new Point3D(locationData.Newlocation.X, locationData.Newlocation.Y, 0), new Point3D(locationData.Oldlocation.X, locationData.Oldlocation.Y, 0)) * -1;

            locationData.PlayerLocationCircle.ArrowLine.RotateTransform = new RotateTransform(angle);
            Canvas.SetLeft(locationData.PlayerLocationCircle.ArrowLine, -(locationData.Newlocation.Y + locationData.MapOffset.X) * locationData.CurrentScaling);
            Canvas.SetTop(locationData.PlayerLocationCircle.ArrowLine, -(locationData.Newlocation.X + locationData.MapOffset.Y) * locationData.CurrentScaling);

            var heighdiv2 = locationData.PlayerLocationCircle.Ellipse.Height / 2 / locationData.CurrentScaling;
            Canvas.SetLeft(locationData.PlayerLocationCircle.Ellipse, -(locationData.Newlocation.Y + locationData.MapOffset.X + heighdiv2) * locationData.CurrentScaling);
            Canvas.SetTop(locationData.PlayerLocationCircle.Ellipse, -(locationData.Newlocation.X + locationData.MapOffset.Y + heighdiv2) * locationData.CurrentScaling);

            Canvas.SetLeft(locationData.PlayerLocationCircle.PlayerName, -(locationData.Newlocation.Y + locationData.MapOffset.X) * locationData.CurrentScaling);
            Canvas.SetTop(locationData.PlayerLocationCircle.PlayerName, -(locationData.Newlocation.X + locationData.MapOffset.Y + heighdiv2) * locationData.CurrentScaling);

            if (!locationData.Trackingdistance.HasValue)
            {
                locationData.PlayerLocationCircle.TrackingEllipse.Visibility = Visibility.Hidden;
            }
            else
            {
                locationData.PlayerLocationCircle.TrackingEllipse.Visibility = Visibility.Visible;
                locationData.PlayerLocationCircle.TrackingEllipse.Height = locationData.Trackingdistance.Value;
                locationData.PlayerLocationCircle.TrackingEllipse.Width = locationData.Trackingdistance.Value;
            }

            heighdiv2 = locationData.PlayerLocationCircle.TrackingEllipse.Height / 2;
            Canvas.SetLeft(locationData.PlayerLocationCircle.TrackingEllipse, -(locationData.Newlocation.Y + locationData.MapOffset.X + heighdiv2) * locationData.CurrentScaling);
            Canvas.SetTop(locationData.PlayerLocationCircle.TrackingEllipse, -(locationData.Newlocation.X + locationData.MapOffset.Y + heighdiv2) * locationData.CurrentScaling);

            var transform = new MatrixTransform();
            var translation = new TranslateTransform(locationData.Transform.Value.OffsetX, locationData.Transform.Value.OffsetY);
            transform.Matrix = locationData.PlayerLocationCircle.ArrowLine.RotateTransform.Value * translation.Value;
            locationData.PlayerLocationCircle.ArrowLine.RenderTransform = transform;

            var transform2 = new MatrixTransform();
            var translation2 = new TranslateTransform(locationData.Transform.Value.OffsetX, locationData.Transform.Value.OffsetY);
            transform2.Matrix = translation2.Value;
            locationData.PlayerLocationCircle.Ellipse.RenderTransform = transform2;

            locationData.PlayerLocationCircle.TrackingEllipse.RenderTransform = locationData.Transform;
        }

        static public PlayerLocationCircle AddPlayerToCanvas(AddPlayerToCanvasData toCanvasData)
        {
            var playerlocsize = MathHelper.ChangeRange(Math.Max(toCanvasData.AABB.MaxWidth, toCanvasData.AABB.MaxHeight), 500, 35000, 40, 1750);
            var playerstrokthickness = MathHelper.ChangeRange(Math.Max(toCanvasData.AABB.MaxWidth, toCanvasData.AABB.MaxHeight), 500, 35000, 3, 40);
            var color = System.Windows.Media.Color.FromRgb(61, 235, 52);
            if (toCanvasData.Name != "You")
            {
                var minrange = 40;
                var random = new Random(DateTime.Now.Millisecond);
                color = System.Windows.Media.Color.FromRgb(
                (byte)random.Next(minrange, 235),
                (byte)random.Next(minrange, 235),
                (byte)random.Next(minrange, 235));
            }

            var playerloc = new PlayerLocationCircle
            {
                PlayerName = new TextBlock
                {
                    Text = toCanvasData.Name,
                    FontSize = OtherLabelFontSize(toCanvasData.AABB),
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
                    RenderTransform = toCanvasData.Transform
                },
                Ellipse = new Ellipse()
                {
                    Height = playerlocsize / 4,
                    Width = playerlocsize / 4,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = playerstrokthickness,
                    RenderTransform = new RotateTransform()
                },
                ArrowLine = new ArrowLine()
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = playerstrokthickness,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = playerlocsize,
                    ArrowLength = playerlocsize / 4,
                    ArrowEnds = ArrowEnds.End,
                    RotateTransform = new RotateTransform()
                },
                TrackingEllipse = new Ellipse()
                {
                    Height = toCanvasData.Trackingdistance ?? 100,
                    Width = toCanvasData.Trackingdistance ?? 100,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = playerstrokthickness,
                    RenderTransform = new RotateTransform(),
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(5, color.R, color.G, color.B)),
                    Visibility = toCanvasData.Trackingdistance == null ? Visibility.Hidden : Visibility.Visible
                }
            };
            MapViewModelService.AddToCanvas(playerloc, toCanvasData.Canvas, toCanvasData.AABB);
            return playerloc;
        }

        static public void AddToCanvas(PlayerLocationCircle playerLocationCircle, Canvas canvas, MapLoad.AABB aabb)
        {
            var playerlocsize = MathHelper.ChangeRange(Math.Max(aabb.MaxWidth, aabb.MaxHeight), 500, 35000, 40, 1750);
            if (playerLocationCircle.Ellipse != null)
            {
                _ = canvas.Children.Add(playerLocationCircle.Ellipse);
                Canvas.SetLeft(playerLocationCircle.Ellipse, aabb.Center.X + playerLocationCircle.Ellipse.Height + (playerLocationCircle.Ellipse.Height / 2));
                Canvas.SetTop(playerLocationCircle.Ellipse, aabb.Center.Y + playerLocationCircle.Ellipse.Height + (playerLocationCircle.Ellipse.Height / 2));
            }
            if (playerLocationCircle.PlayerName != null)
            {
                _ = canvas.Children.Add(playerLocationCircle.PlayerName);
                Canvas.SetLeft(playerLocationCircle.PlayerName, aabb.Center.X + (playerlocsize / 2));
                Canvas.SetTop(playerLocationCircle.PlayerName, aabb.Center.Y + (playerlocsize / 2));
            }
            if (playerLocationCircle.ArrowLine != null)
            {
                _ = canvas.Children.Add(playerLocationCircle.ArrowLine);
                Canvas.SetLeft(playerLocationCircle.ArrowLine, aabb.Center.X + (playerlocsize / 2));
                Canvas.SetTop(playerLocationCircle.ArrowLine, aabb.Center.Y + (playerlocsize / 2));
            }

            if (playerLocationCircle.TrackingEllipse != null)
            {
                _ = canvas.Children.Add(playerLocationCircle.TrackingEllipse);
                Canvas.SetLeft(playerLocationCircle.TrackingEllipse, aabb.Center.X + playerLocationCircle.TrackingEllipse.Height + (playerLocationCircle.TrackingEllipse.Height / 2));
                Canvas.SetTop(playerLocationCircle.TrackingEllipse, aabb.Center.Y + playerLocationCircle.TrackingEllipse.Height + (playerLocationCircle.TrackingEllipse.Height / 2));
            }
        }
    }
}
