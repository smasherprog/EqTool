using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQTool.Shapes;
using System;
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

        public MapViewModel(MapLoad mapLoad, ActivePlayer activePlayer)
        {
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
        }

        public Point3D Lastlocation = new Point3D(0, 0, 0);
        public AABB AABB = new AABB();
        public Point3D MapOffset = new Point3D(0, 0, 0);

        public ArrowLine PlayerLocationIcon { get; set; }

        public Ellipse PlayerLocationCircle { get; set; }

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

        private bool MapLoading = false;
        private Point _LastMouselocation = new Point(0, 0);

        private Point LastMouselocation
        {
            get => _LastMouselocation;
            set
            {
                _LastMouselocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MouseLocation));
            }
        }

        public string MouseLocation => $"   {LastMouselocation.Y:0.##}, {LastMouselocation.X:0.##}";

        private string LoadedZone = string.Empty;

        public double ZoneLabelFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 20, 170);
        public double OtherLabelFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 10, 110);
        public double SmallFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 7, 50);

        public bool LoadMap(string zone, PanAndZoomCanvas canvas)
        {
            if (MapLoading)
            {
                return false;
            }

            try
            {
                MapLoading = true;
                if (string.IsNullOrWhiteSpace(zone) || zone == LoadedZone)
                {
                    return false;
                }
                zone = ZoneParser.TranslateToMapName(zone);
                if (string.IsNullOrWhiteSpace(zone))
                {
                    zone = "freportw";
                }
                var stop = new Stopwatch();
                stop.Start();
                var map = mapLoad.Load(zone);
                stop.Stop();
                Debug.WriteLine($"Time to load {zone} - {stop.ElapsedMilliseconds}ms");
                if (map.Labels.Any() || map.Lines.Any())
                {
                    canvas.Reset(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), zone);
                    AABB = map.AABB;
                    LoadedZone = Title = zone;
                    Debug.WriteLine($"Loading: {zone}");
                    //var colordic = new Dictionary<System.Windows.Media.Color, Tuple<EQMapColor, SolidColorBrush>>();
                    //foreach (var group in map.Lines)
                    //{
                    //    if (!colordic.ContainsKey(group.Color))
                    //    {
                    //        var c = EQMapColor.GetThemedColors(group.Color);
                    //        colordic[group.Color] = Tuple.Create(c, new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor));
                    //    }
                    //}
                    //foreach (var group in map.Labels)
                    //{
                    //    if (!colordic.ContainsKey(group.Color))
                    //    {
                    //        var c = EQMapColor.GetThemedColors(group.Color);
                    //        colordic[group.Color] = Tuple.Create(c, new SolidColorBrush(App.Theme == Themes.Light ? c.LightColor : c.DarkColor));
                    //    }
                    //}
                    MapOffset = map.Offset;
                    var linethickness = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 800, 35000, 2, 40);
                    canvas.Children.Clear();
                    foreach (var group in map.Lines)
                    {
                        var c = EQMapColor.GetThemedColors(group.Color);
                        group.ThemeColors = c;
                        var colorstuff = new SolidColorBrush(c.DarkColor);
                        var l = new Line
                        {
                            Tag = group,
                            X1 = group.Points[0].X,
                            Y1 = group.Points[0].Y,
                            X2 = group.Points[1].X,
                            Y2 = group.Points[1].Y,
                            StrokeThickness = linethickness,
                            Stroke = colorstuff,
                            RenderTransform = canvas.Transform
                        };
                        _ = canvas.Children.Add(l);
                    }

                    canvas.Height = Math.Abs(map.AABB.MaxHeight);
                    canvas.Width = Math.Abs(map.AABB.MaxWidth);
                    Debug.WriteLine($"Labels: {map.Labels.Count}");
                    foreach (var item in map.Labels)
                    {
                        var text = new TextBlock
                        {
                            Tag = item,
                            Text = item.label.Replace('_', ' '),
                            FontSize = item.LabelSize == LabelSize.Large ? ZoneLabelFontSize : OtherLabelFontSize,
                            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
                            RenderTransform = canvas.Transform
                        };
                        var circle = new Ellipse()
                        {
                            Tag = item,
                            Width = 10,
                            Height = 10,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3
                        };
                        _ = canvas.Children.Add(circle);
                        _ = canvas.Children.Add(text);
                        Canvas.SetLeft(text, item.Point.X);
                        Canvas.SetTop(text, item.Point.Y);
                        Canvas.SetLeft(circle, item.Point.X);
                        Canvas.SetTop(circle, item.Point.Y);
                    }

                    var playerlocsize = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 500, 35000, 40, 1750);
                    var playerstrokthickness = MathHelper.ChangeRange(Math.Max(map.AABB.MaxWidth, map.AABB.MaxHeight), 500, 35000, 3, 40);
                    PlayerLocationCircle = new Ellipse
                    {
                        Name = "PlayerLocationCircle",
                        Height = playerlocsize / 4,
                        Width = playerlocsize / 4,
                        Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                        StrokeThickness = playerstrokthickness,
                        RenderTransform = new RotateTransform()
                    };

                    _ = canvas.Children.Add(PlayerLocationCircle);
                    Canvas.SetLeft(PlayerLocationCircle, AABB.Center.X + PlayerLocationCircle.Height + (PlayerLocationCircle.Height / 2));
                    Canvas.SetTop(PlayerLocationCircle, AABB.Center.Y + PlayerLocationCircle.Height + (PlayerLocationCircle.Height / 2));

                    PlayerLocationIcon = new ArrowLine
                    {
                        Name = "PlayerLocation",
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

                    _ = canvas.Children.Add(PlayerLocationIcon);
                    Canvas.SetLeft(PlayerLocationIcon, AABB.Center.X + (playerlocsize / 2));
                    Canvas.SetTop(PlayerLocationIcon, AABB.Center.Y + (playerlocsize / 2));

                    return true;
                }

                return false;
            }
            finally
            {
                MapLoading = false;
            }
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

        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        public void MoveToPlayerLocation(MapWidget mw, MatrixTransform transform)
        {
            Canvas.SetLeft(mw, Canvas.GetLeft(PlayerLocationIcon));
            Canvas.SetTop(mw, Canvas.GetTop(PlayerLocationIcon));
            mw.RenderTransform = transform;
        }

        private void SetOpacity(Line l, double v)
        {
            if (l.Stroke.IsFrozen)
            {
                l.Stroke = l.Stroke.Clone();
            }

            l.Stroke.Opacity = v;
        }

        private void SetOpacity(Ellipse l, double v)
        {
            if (l.Stroke.IsFrozen)
            {
                l.Stroke = l.Stroke.Clone();
            }

            l.Stroke.Opacity = v;
        }

        private void SetOpacity(TextBlock l, double v)
        {
            if (l.Foreground.IsFrozen)
            {
                l.Foreground = l.Foreground.Clone();
            }

            l.Foreground.Opacity = v;
        }

        public void UpdateLocation(Point3D value1, PanAndZoomCanvas canvas)
        {
            if (MapLoading || PlayerLocationIcon == null)
            {
                return;
            }

            OnPropertyChanged(nameof(Title));
            var newdir = new Point3D(value1.X, value1.Y, 0) - new Point3D(Lastlocation.X, Lastlocation.Y, 0);
            newdir.Normalize();
            var angle = GetAngleBetweenPoints(new Point3D(value1.X, value1.Y, 0), new Point3D(Lastlocation.X, Lastlocation.Y, 0)) * -1;
            Lastlocation = value1;
            PlayerLocationIcon.RotateTransform = new RotateTransform(angle);
            Canvas.SetLeft(PlayerLocationIcon, -(value1.Y + MapOffset.X) * canvas.CurrentScaling);
            Canvas.SetTop(PlayerLocationIcon, -(value1.X + MapOffset.Y) * canvas.CurrentScaling);
            var heighdiv2 = PlayerLocationCircle.Height / 2 / canvas.CurrentScaling;
            Canvas.SetLeft(PlayerLocationCircle, -(value1.Y + MapOffset.X + heighdiv2) * canvas.CurrentScaling);
            Canvas.SetTop(PlayerLocationCircle, -(value1.X + MapOffset.Y + heighdiv2) * canvas.CurrentScaling);
            var transform = new MatrixTransform();
            var translation = new TranslateTransform(canvas.Transform.Value.OffsetX, canvas.Transform.Value.OffsetY);
            transform.Matrix = PlayerLocationIcon.RotateTransform.Value * translation.Value;
            PlayerLocationIcon.RenderTransform = transform;
            var transform2 = new MatrixTransform();
            _ = new TranslateTransform(canvas.Transform.Value.OffsetX, canvas.Transform.Value.OffsetY);
            transform2.Matrix = translation.Value;
            PlayerLocationCircle.RenderTransform = transform2;
            var zoneinfo = ZoneParser.ZoneInfoMap[LoadedZone];
            if (!zoneinfo.ShowAllMapLevels && canvas.Children.Count > 0)
            {
                var lastloc = new Point3D(-(value1.Y + MapOffset.X), -(value1.X + MapOffset.Y), Lastlocation.Z);
                var twiceheight = zoneinfo.ZoneLevelHeight * 2;
                foreach (var child in canvas.Children)
                {
                    if (child is Line a)
                    {
                        var m = a.Tag as MapLine;
                        var shortestdistance = Math.Abs(m.Points[0].Z - lastloc.Z);
                        shortestdistance = Math.Min(Math.Abs(m.Points[1].Z - lastloc.Z), shortestdistance);

                        if (shortestdistance < zoneinfo.ZoneLevelHeight)
                        {
                            SetOpacity(a, 1);
                        }
                        else if (shortestdistance >= zoneinfo.ZoneLevelHeight && shortestdistance <= twiceheight + zoneinfo.ZoneLevelHeight)
                        {
                            var dist = ((shortestdistance - zoneinfo.ZoneLevelHeight) * -1) + twiceheight; //changed range to [0,80] with 0 being the FURTHest distance
                            dist = (dist / twiceheight) + .1; // scale to [.1,1.1] 
                            _ = Clamp(dist, .1, 1);
                            SetOpacity(a, dist);
                        }
                        else
                        {
                            SetOpacity(a, .1);
                        }
                    }
                    else if (child is TextBlock t)
                    {
                        var m = t.Tag as MapLabel;
                        var shortestdistance = Math.Abs(m.Point.Z - lastloc.Z);
                        shortestdistance = Math.Min(Math.Abs(m.Point.Z - lastloc.Z), shortestdistance);

                        if (shortestdistance < zoneinfo.ZoneLevelHeight)
                        {
                            SetOpacity(t, 1);
                        }
                        else if (shortestdistance >= zoneinfo.ZoneLevelHeight && shortestdistance <= twiceheight + zoneinfo.ZoneLevelHeight)
                        {
                            var dist = ((shortestdistance - zoneinfo.ZoneLevelHeight) * -1) + twiceheight; //changed range to [0,80] with 0 being the FURTHest distance
                            dist = (dist / twiceheight) + .1; // scale to [.1,1.1] 
                            _ = Clamp(dist, .1, 1);
                            SetOpacity(t, dist);
                        }
                        else
                        {
                            SetOpacity(t, .1);
                        }
                    }
                    else if (child is Ellipse e)
                    {
                        if (e != PlayerLocationCircle)
                        {
                            var m = e.Tag as MapLabel;
                            var shortestdistance = Math.Abs(m.Point.Z - lastloc.Z);
                            shortestdistance = Math.Min(Math.Abs(m.Point.Z - lastloc.Z), shortestdistance);

                            if (shortestdistance < zoneinfo.ZoneLevelHeight)
                            {
                                SetOpacity(e, 1);
                            }
                            else if (shortestdistance >= zoneinfo.ZoneLevelHeight && shortestdistance <= twiceheight + zoneinfo.ZoneLevelHeight)
                            {
                                var dist = ((shortestdistance - zoneinfo.ZoneLevelHeight) * -1) + twiceheight; //changed range to [0,80] with 0 being the FURTHest distance
                                dist = (dist / twiceheight) + .1; // scale to [.1,1.1] 
                                _ = Clamp(dist, .1, 1);
                                SetOpacity(e, dist);
                            }
                            else
                            {
                                SetOpacity(e, .1);
                            }
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void MouseMove(Point mousePosition, object sender, System.Windows.Input.MouseEventArgs e)
        {
            mousePosition.Y += MapOffset.Y;
            mousePosition.X += MapOffset.X;
            mousePosition.Y *= -1;
            mousePosition.X *= -1;
            LastMouselocation = mousePosition;
        }
    }
}
