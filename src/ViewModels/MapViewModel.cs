using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static EQTool.Services.MapLoad;

namespace EQTool.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;

        public MapViewModel(MapLoad mapLoad, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
        }

        private Point Lastlocation = new Point(0, 0);
        public AABB AABB = new AABB();
        private Point3D MapOffset = new Point3D(0, 0, 0);

        public Polyline PlayerLocationIcon { get; set; }

        private string _Title = string.Empty;

        public string Title
        {
            get => _Title + $"   {_MouseWorldCoordinates.X:0.##}, {_MouseWorldCoordinates.Y:0.##}, {_MouseWorldCoordinates.Z:0.##}";
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }

        public Point3D _MouseWorldCoordinates;

        public Point3D MouseWorldCoordinates
        {
            get => _MouseWorldCoordinates;
            set
            {
                _MouseWorldCoordinates = value;
                //Debug.WriteLine($"MouseWorldCoordinates: {MouseWorldCoordinates}");
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public bool LoadMap(string zone, PanAndZoomCanvas canvas)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                return false;
            }

            Title = zone;
            Debug.WriteLine($"Loading: {zone}");
            var map = mapLoad.Load(zone);
            if (map.Labels.Any() || map.Lines.Any())
            {
                MapOffset = map.Offset;
                var linethickness = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 1000, 35000, 2, 10);
                canvas.Children.Clear();
                foreach (var group in map.Lines)
                {
                    var c = EQMapColor.GetThemedColors(group.Color);
                    var l = new Line
                    {
                        Tag = c,
                        X1 = group.Points[0].X,
                        Y1 = group.Points[0].Y,
                        X2 = group.Points[1].X,
                        Y2 = group.Points[1].Y,
                        StrokeThickness = linethickness,
                        Stroke = new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor)
                    };
                    _ = canvas.Children.Add(l);
                }

                canvas.Height = Math.Abs(map.AABB.MaxHeight);
                canvas.Width = Math.Abs(map.AABB.MaxWidth);
                Debug.WriteLine($"Labels: {map.Labels.Count}");
                foreach (var item in map.Labels)
                {
                    var c = EQMapColor.GetThemedColors(item.Color);
                    var text = new TextBlock
                    {
                        Tag = c,
                        Text = item.label.Replace('_', ' '),
                        Foreground = new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor),
                        Height = 50
                    };
                    _ = canvas.Children.Add(text);
                    Canvas.SetLeft(text, item.Point.X);
                    Canvas.SetTop(text, item.Point.Y);
                }

                PlayerLocationIcon = new Polyline
                {
                    Points = new PointCollection(new List<Point>
                     {
                        new Point(25, 25),
                        new Point(0,50),
                        new Point(25,75),
                        new Point(50,50),
                        new Point(25,25),
                        new Point(25,0)
                     }),
                    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                    StrokeThickness = 15
                };
                AABB = map.AABB;
                _ = canvas.Children.Add(PlayerLocationIcon);
                Canvas.SetLeft(PlayerLocationIcon, AABB.Center.X);
                Canvas.SetTop(PlayerLocationIcon, AABB.Center.Y);
                return true;
            }

            return false;
        }
        private static Point3D RotatePoint(Point3D pointToRotate, Point3D centerPoint, double angleInDegrees)
        {
            var angleInRadians = angleInDegrees * (Math.PI / 180);
            var cosTheta = Math.Cos(angleInRadians);
            var sinTheta = Math.Sin(angleInRadians);
            return new Point3D
            {
                X =
                    (int)
                    ((cosTheta * (pointToRotate.X - centerPoint.X)) -
                    (sinTheta * (pointToRotate.Y - centerPoint.Y)) + centerPoint.X),
                Y =
                    (int)
                    ((sinTheta * (pointToRotate.X - centerPoint.X)) +
                    (cosTheta * (pointToRotate.Y - centerPoint.Y)) + centerPoint.Y)
            };
        }

        public bool LoadDefaultMap(PanAndZoomCanvas canvas)
        {
            _ = activePlayer.Update();
            var z = ZoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            return LoadMap(z, canvas);
        }

        public void UpdateLocation(Point3D value1, PanAndZoomCanvas canvas)
        {
            //value1 = RotatePoint(value1, new Point3D(AABBCenter.X, AABBCenter.Y, 0), 180);

            //var vec = newval - new Point3D(Lastlocation.Value.X, Lastlocation.Value.Y);
            //vec.Normalize();
            //var endpos = ((vec * 20) + newval.ToVector3D()).ToPoint3D();
            Lastlocation = new Point(value1.X + MapOffset.Y, value1.Y + MapOffset.X);

            Canvas.SetLeft(PlayerLocationIcon, -Lastlocation.Y * canvas.CurrentScaling);
            Canvas.SetTop(PlayerLocationIcon, -Lastlocation.X * canvas.CurrentScaling);

            PlayerLocationIcon.RenderTransform = new TranslateTransform(canvas.Transform.Value.OffsetX, canvas.Transform.Value.OffsetY);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
