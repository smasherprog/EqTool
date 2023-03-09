using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQTool.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public MapViewModel(MapLoad mapLoad, ActivePlayer activePlayer)
        {
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
        }

        private Point3D Lastlocation = new Point3D(0, 0, 0);
        public AABB AABB = new AABB();
        private Point3D MapOffset = new Point3D(0, 0, 0);

        public ArrowLine PlayerLocationIcon { get; set; }

        private string _Title = string.Empty;

        public string Title
        {
            get => _Title + "  v" + App.Version + $"   {Lastlocation.X:0.##}, {Lastlocation.Y:0.##}, {Lastlocation.Z:0.##}";
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }

        private string LoadedZone = string.Empty;

        public class Mapdata : MapLine
        {
            public EQMapColor MapColor { get; set; }
        }

        public bool LoadMap(string zone, PanAndZoomCanvas canvas)
        {
            if (string.IsNullOrWhiteSpace(zone) || zone == LoadedZone)
            {
                return false;
            }

            var map = mapLoad.Load(zone);
            if (map.Labels.Any() || map.Lines.Any())
            {
                LoadedZone = Title = zone;
                Debug.WriteLine($"Loading: {zone}");
                var colordic = new Dictionary<System.Windows.Media.Color, Tuple<EQMapColor, SolidColorBrush>>();
                foreach (var group in map.Lines)
                {
                    if (!colordic.ContainsKey(group.Color))
                    {
                        var c = EQMapColor.GetThemedColors(group.Color);
                        colordic[group.Color] = Tuple.Create(c, new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor));
                    }
                }
                foreach (var group in map.Labels)
                {
                    if (!colordic.ContainsKey(group.Color))
                    {
                        var c = EQMapColor.GetThemedColors(group.Color);
                        colordic[group.Color] = Tuple.Create(c, new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor));
                    }
                }
                MapOffset = map.Offset;
                var linethickness = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 800, 35000, 2, 40);
                canvas.Children.Clear();
                foreach (var group in map.Lines)
                {
                    var colorstuff = colordic[group.Color];
                    var d = new Mapdata
                    {
                        MapColor = colordic[group.Color].Item1,
                        Points = group.Points,
                        Color = group.Color
                    };
                    var l = new Line
                    {
                        Tag = d,
                        X1 = group.Points[0].X,
                        Y1 = group.Points[0].Y,
                        X2 = group.Points[1].X,
                        Y2 = group.Points[1].Y,
                        StrokeThickness = linethickness,
                        Stroke = colorstuff.Item2,
                        RenderTransform = canvas.Transform
                    };
                    _ = canvas.Children.Add(l);
                }

                canvas.Height = Math.Abs(map.AABB.MaxHeight);
                canvas.Width = Math.Abs(map.AABB.MaxWidth);
                Debug.WriteLine($"Labels: {map.Labels.Count}");
                var labelscaling = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 500, 35000, 40, 200);
                foreach (var item in map.Labels)
                {
                    var colorstuff = colordic[item.Color];
                    var d = new Mapdata
                    {
                        MapColor = colordic[item.Color].Item1,
                        Points = new[] { item.Point },
                        Color = item.Color
                    };
                    var text = new TextBlock
                    {
                        Tag = d,
                        Text = item.label.Replace('_', ' '),
                        Height = labelscaling,
                        Foreground = colorstuff.Item2,
                        RenderTransform = canvas.Transform
                    };
                    _ = canvas.Children.Add(text);
                    Canvas.SetLeft(text, item.Point.X);
                    Canvas.SetTop(text, item.Point.Y);
                }

                var playerlocsize = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 500, 35000, 40, 1750);
                var playerstrokthickness = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 500, 35000, 3, 40);
                //PlayerLocationIcon = new Polyline
                //{
                //    Points = new PointCollection(new List<Point>
                //     {
                //      new Point(25, 25),
                //        new Point(0,50),
                //        new Point(25,75),
                //        new Point(50,50),
                //        new Point(25,25),
                //        new Point(25,0)
                //     }),
                //    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                //    StrokeThickness = 20
                //};
                PlayerLocationIcon = new ArrowLine
                {
                    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                    StrokeThickness = playerstrokthickness,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = playerlocsize,
                    ArrowLength = playerlocsize / 4,
                    ArrowEnds = ArrowEnds.End,
                    RotateTransform = new RotateTransform()
                };
                //PlayerLocationIcon = new Ellipse
                //{
                //    Height = playerlocsize,
                //    Width = playerlocsize,
                //    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                //    StrokeThickness = playerstrokthickness
                //};

                AABB = map.AABB;
                _ = canvas.Children.Add(PlayerLocationIcon);
                Canvas.SetLeft(PlayerLocationIcon, AABB.Center.X + (playerlocsize / 2));
                Canvas.SetTop(PlayerLocationIcon, AABB.Center.Y + (playerlocsize / 2));
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

        public static double GetAngleBetweenPoints(Point3D pt1, Point3D pt2)
        {
            var dx = pt2.X - pt1.X;
            var dy = pt2.Y - pt1.Y;
            var deg = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (deg < 0) { deg += 360; }

            return deg;
        }

        public void UpdateLocation(Point3D value1, PanAndZoomCanvas canvas)
        {
            OnPropertyChanged(nameof(Title));
            var newdir = new Point3D(value1.X, value1.Y, 0) - new Point3D(Lastlocation.X, Lastlocation.Y, 0);
            newdir.Normalize();
            var angle = GetAngleBetweenPoints(new Point3D(value1.X, value1.Y, 0), new Point3D(Lastlocation.X, Lastlocation.Y, 0)) * -1;
            Lastlocation = value1;
            PlayerLocationIcon.RotateTransform = new RotateTransform(angle);
            Canvas.SetLeft(PlayerLocationIcon, -(value1.Y + MapOffset.X) * canvas.CurrentScaling);
            Canvas.SetTop(PlayerLocationIcon, -(value1.X + MapOffset.Y) * canvas.CurrentScaling);
            var transform = new MatrixTransform();
            var translation = new TranslateTransform(canvas.Transform.Value.OffsetX, canvas.Transform.Value.OffsetY);
            transform.Matrix = PlayerLocationIcon.RotateTransform.Value * translation.Value;
            PlayerLocationIcon.RenderTransform = transform;
            if (ZoneParser.NoZoneZHelping[LoadedZone] && canvas.Children.Count > 0)
            {
                var lastloc = new Point3D(-(value1.Y + MapOffset.X), -(value1.X + MapOffset.Y), Lastlocation.Z);
                var closest = canvas.Children[0] as Line;
                var closestmapdata = closest.Tag as Mapdata;
                var cloestdist = (closestmapdata.Points[0] - lastloc).LengthSquared;
                foreach (var child in canvas.Children)
                {
                    if (child is Line a)
                    {
                        var m = a.Tag as Mapdata;
                        var possible = (m.Points[0] - lastloc).LengthSquared;
                        //Debug.WriteLine($"{m.Points[0]} - {lastloc} = {possible}");
                        if (possible < cloestdist)
                        {
                            //Debug.WriteLine($"Lower {possible} {cloestdist}");
                            cloestdist = possible;
                            closest = a;
                        }
                        possible = (m.Points[1] - lastloc).LengthSquared;
                        //Debug.WriteLine($"{m.Points[1]} - {lastloc} = {possible}");
                        if (possible < cloestdist)
                        {
                            //Debug.WriteLine($"Lower {possible} {cloestdist}");
                            cloestdist = possible;
                            closest = a;
                        }
                    }
                }
                closestmapdata = closest.Tag as Mapdata;
                //closest.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                foreach (var child in canvas.Children)
                {
                    if (child is Line a)
                    {
                        var m = a.Tag as Mapdata;
                        a.Stroke.Opacity = m.MapColor.OriginalColor != closestmapdata.MapColor.OriginalColor ? .25 : 1;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
