using EQTool.Models;
using EQTool.Services;
using EQTool.Shapes;
using EQToolShared.Enums;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static EQTool.Services.MapLoad;

namespace EQTool.ViewModels
{
    public class PlayerLocationCircle : INotifyPropertyChanged
    {
        public TextBlock PlayerName;
        public Ellipse Ellipse;
        public ArrowLine ArrowLine;
        public Ellipse TrackingEllipse;
        private Brush _Color;

        public Brush Color
        {
            get => _Color;
            set
            {
                _Color = value;
                OnPropertyChanged();
            }
        }

        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class PlayerLocation : PlayerLocationCircle
    {
        public SignalrPlayer Player;

    }

    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private MatrixTransform Transform = new MatrixTransform();
        private MatrixTransform EllipseTransform = new MatrixTransform();
        private Point _initialMousePosition;
        private Point _mouseuppoint;
        private Point3D MapOffset = new Point3D(0, 0, 0);
        private bool MapLoading = false;
        private PlayerLocationCircle PlayerLocation;
        public ObservableCollection<PlayerLocation> Players { get; set; }
        private Canvas Canvas;
        private float CurrentScaling = 1.0f;
        private readonly float Zoomfactor = 1.1f;
        private bool _dragging;
        private UIElement _selectedElement;
        private Vector _draggingDelta;
        private bool TimerOpen = false;
        private readonly TimersService timersService;
        private bool CenterOnPlayer = false;
        public Point CenterRelativeToCanvas = new Point(0, 0);

        public string MouseLocation => $"   {LastMouselocation.Y:0.##}, {LastMouselocation.X:0.##}";

        public AABB AABB = new AABB();

        public MapViewModel(MapLoad mapLoad, ActivePlayer activePlayer, LoggingService loggingService, TimersService timersService)
        {
            this.Players = new ObservableCollection<PlayerLocation>();
            this.timersService = timersService;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
        }

        private TimeSpan _TimerValue = TimeSpan.FromMinutes(72);
        public TimeSpan TimerValue
        {
            get => _TimerValue;
            set
            {
                _TimerValue = value;
                OnPropertyChanged();
            }
        }

        private Point3D _Lastlocation = new Point3D(0, 0, 0);
        public Point3D Lastlocation
        {
            get => _Lastlocation;
            set
            {
                _Lastlocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => _ZoneName + "  v" + App.Version + $"   {Lastlocation.X:0.##}, {Lastlocation.Y:0.##}, {Lastlocation.Z:0.##}";

        private string _ZoneName = string.Empty;

        public string ZoneName
        {
            get => _ZoneName;
            set
            {
                _ZoneName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

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

        private void Reset()
        {
            Transform = new MatrixTransform();
            EllipseTransform = new MatrixTransform();
            CurrentScaling = 1.0f;
            Canvas?.Children?.Clear();
            Players.Clear();
        }

        public void ToggleCenter()
        {
            this.CenterOnPlayer = !this.CenterOnPlayer;
            CenterMapOnPlayer(Lastlocation);
        }
        private void CenterMapOnPlayer(Point3D value1)
        {
            if (CenterOnPlayer && CurrentScaling != 1.0f)
            {
                var xScale = Lastlocation.X - value1.X;
                var yScale = Lastlocation.Y - value1.Y;
                MoveMap((int)(yScale * -1), (int)(xScale * -1));
            }
        }

        public bool LoadMap(string zone, Canvas canvas)
        {
            if (MapLoading)
            {
                return false;
            }
            zone = ZoneParser.TranslateToMapName(zone);
            if (string.IsNullOrWhiteSpace(zone))
            {
                zone = "freportw";
            }
            if (zone == ZoneName)
            {
                return false;
            }
            Canvas = canvas;
            MapLoading = true;
            var stop = new Stopwatch();
            stop.Start();
            try
            {
                var map = mapLoad.Load(zone);
                if (map.Labels.Any() || map.Lines.Any())
                {
                    Reset();
                    AABB = map.AABB;
                    ZoneName = zone;
                    Debug.WriteLine($"Loading: {zone}");
                    MapOffset = map.Offset;
                    var linethickness = MapViewModelService.MapLinethickness(this.AABB);
                    foreach (var item in map.Lines)
                    {
                        var c = EQMapColor.GetThemedColors(item.Color);
                        var colorstuff = new SolidColorBrush(c.DarkColor);
                        var l = new Line
                        {
                            Tag = item,
                            X1 = item.Points[0].X,
                            Y1 = item.Points[0].Y,
                            X2 = item.Points[1].X,
                            Y2 = item.Points[1].Y,
                            StrokeThickness = linethickness,
                            Stroke = colorstuff,
                            RenderTransform = Transform
                        };
                        _ = canvas.Children.Add(l);
                    }

                    Debug.WriteLine($"Labels: {map.Labels.Count}");
                    var locationdotsize = MapViewModelService.PlayerEllipsesSize(this.AABB);
                    var locationthickness = MapViewModelService.PlayerEllipsesThickness(this.AABB);
                    var zoneLabelFontSize = MapViewModelService.ZoneLabelFontSize(this.AABB);
                    var otherLabelFontSize = MapViewModelService.OtherLabelFontSize(this.AABB);
                    foreach (var item in map.Labels)
                    {
                        var text = new TextBlock
                        {
                            Tag = item,
                            Text = item.label.Replace('_', ' '),
                            FontSize = item.LabelSize == LabelSize.Large ? zoneLabelFontSize : otherLabelFontSize,
                            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
                            RenderTransform = Transform
                        };
                        var circle = new Ellipse()
                        {
                            Tag = item,
                            Width = locationdotsize,
                            Height = locationdotsize,
                            Stroke = Brushes.Red,
                            StrokeThickness = locationthickness,
                            RenderTransform = EllipseTransform
                        };
                        _ = canvas.Children.Add(circle);
                        _ = canvas.Children.Add(text);
                        Canvas.SetLeft(text, item.Point.X);
                        Canvas.SetTop(text, item.Point.Y);
                        Canvas.SetLeft(circle, item.Point.X);
                        Canvas.SetTop(circle, item.Point.Y);
                    }

                    PlayerLocation = MapViewModelService.AddPlayerToCanvas(new AddPlayerToCanvasData
                    {
                        Name = "You",
                        Canvas = Canvas,
                        Trackingdistance = this.activePlayer?.Player?.TrackingDistance,
                        AABB = this.AABB,
                        Transform = Transform
                    });
                    MapViewModelService.UpdateLocation(new UpdateLocationData
                    {
                        Trackingdistance = this.activePlayer?.Player?.TrackingDistance,
                        CurrentScaling = CurrentScaling,
                        MapOffset = MapOffset,
                        Oldlocation = Lastlocation,
                        Newlocation = Lastlocation,
                        PlayerLocationCircle = PlayerLocation,
                        Transform = Transform,
                    });
                    PlayerLocation.ArrowLine.Visibility = Visibility.Hidden;
                    PlayerLocation.Ellipse.Visibility = Visibility.Hidden;
                    PlayerLocation.PlayerName.Visibility = Visibility.Hidden;
                    PlayerLocation.TrackingEllipse.Visibility = Visibility.Hidden;
                    var widgets = timersService.LoadTimersForZone(ZoneName);
                    foreach (var mw in widgets)
                    {
                        _ = canvas.Children.Add(mw);
                        Canvas.SetLeft(mw, -mw.TimerInfo.Location.X - MapOffset.X);
                        Canvas.SetTop(mw, -mw.TimerInfo.Location.Y - MapOffset.Y);
                        mw.RenderTransform = new RotateTransform();
                    }
                    return true;
                }

                return false;
            }
            finally
            {
                stop.Stop();
                Debug.WriteLine($"Time to load {zone} - {stop.ElapsedMilliseconds}ms");
                MapLoading = false;
            }
        }

        private PlayerLocation AddPlayerToCanvas(SignalrPlayer signalrPlayer)
        {
            var player = MapViewModelService.AddPlayerToCanvas(new AddPlayerToCanvasData
            {
                Name = signalrPlayer.Name,
                Canvas = Canvas,
                Trackingdistance = signalrPlayer.TrackingDistance,
                AABB = this.AABB,
                Transform = Transform,
            });
            return new PlayerLocation
            {
                ArrowLine = player.ArrowLine,
                Ellipse = player.Ellipse,
                Player = signalrPlayer,
                PlayerName = player.PlayerName,
                TrackingEllipse = player.TrackingEllipse,
                Color = player.Ellipse.Stroke,
                Name = signalrPlayer.Name
            };
        }


        private void RemoveFromCanvas(PlayerLocationCircle playerLocationCircle)
        {
            if (playerLocationCircle.Ellipse != null)
            {
                this.Canvas.Children.Remove(playerLocationCircle.Ellipse);
            }
            if (playerLocationCircle.PlayerName != null)
            {
                this.Canvas.Children.Remove(playerLocationCircle.PlayerName);
            }
            if (playerLocationCircle.ArrowLine != null)
            {
                this.Canvas.Children.Remove(playerLocationCircle.ArrowLine);
            }
            if (playerLocationCircle.TrackingEllipse != null)
            {
                this.Canvas.Children.Remove(playerLocationCircle.TrackingEllipse);
            }
        }

        public bool LoadDefaultMap(Canvas canvas)
        {
            _ = activePlayer.Update();
            var z = ZoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            return LoadMap(z, canvas);
        }


        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        private int failedzonelogcounter = 0;

        public void UpdateLocation(Point3D value1)
        {
            if (MapLoading || PlayerLocation?.ArrowLine == null || Canvas == null || string.IsNullOrWhiteSpace(ZoneName))
            {
                return;
            }

            if (!EQToolShared.Map.ZoneParser.ZoneInfoMap.TryGetValue(ZoneName, out var zoneinfo))
            {
                if (failedzonelogcounter == 0 || failedzonelogcounter++ % 20 == 0)
                {
                    loggingService.Log($"Zone {ZoneName} Not found.", EventType.Error, activePlayer?.Player?.Server);
                }
            }
            PlayerLocation.ArrowLine.Visibility = Visibility.Visible;
            PlayerLocation.Ellipse.Visibility = Visibility.Visible;
            PlayerLocation.PlayerName.Visibility = Visibility.Visible;
            PlayerLocation.TrackingEllipse.Visibility = Visibility.Visible;
            MapViewModelService.UpdateLocation(new UpdateLocationData
            {
                Trackingdistance = this.activePlayer?.Player?.TrackingDistance,
                CurrentScaling = CurrentScaling,
                MapOffset = MapOffset,
                Oldlocation = Lastlocation,
                Newlocation = value1,
                PlayerLocationCircle = PlayerLocation,
                Transform = Transform
            });

            Lastlocation = value1;
            OnPropertyChanged(nameof(Title));
            if (!zoneinfo.ShowAllMapLevels && Canvas.Children.Count > 0)
            {
                var lastloc = new Point3D(-(value1.Y + MapOffset.X), -(value1.X + MapOffset.Y), Lastlocation.Z);
                foreach (var child in Canvas.Children)
                {
                    if (child is Line a)
                    {
                        var m = a.Tag as MapLine;
                        //Debug.WriteLine($"{m.Points[0].Z}");
                        //Debug.WriteLine($"{m.Points[1].Z}");
                        //Debug.WriteLine($"{lastloc.Z}");
                        var shortestdistance = Math.Abs(m.Points[0].Z - lastloc.Z);
                        shortestdistance = Math.Min(Math.Abs(m.Points[1].Z - lastloc.Z), shortestdistance);
                        MapOpacityHelper.AdjustOpacity(shortestdistance, a, zoneinfo);
                    }
                    else if (child is TextBlock t)
                    {
                        MapOpacityHelper.AdjustOpacity(t, zoneinfo, lastloc);
                    }
                    else if (child is Ellipse e)
                    {
                        if (e != PlayerLocation.Ellipse && PlayerLocation.TrackingEllipse != e)
                        {
                            var m = e.Tag as MapLabel;
                            if (m != null)
                            {
                                var shortestdistance = Math.Abs(m.Point.Z - lastloc.Z);
                                MapOpacityHelper.AdjustOpacity(shortestdistance, e, zoneinfo);
                            }
                        }
                    }
                }
            }
            //var translate = new TranslateTransform(x, y);
            //Transform.Matrix = translate.Value * Transform.Matrix;
            //foreach (UIElement child in Canvas.Children)
            //{
            //    if (child is ArrowLine c)
            //    {
            //        var transform = new MatrixTransform();
            //        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
            //        transform.Matrix = c.RotateTransform.Value * translation.Value;
            //        c.RenderTransform = transform;
            //    }
            //    else
            //    {
            //        child.RenderTransform = Transform;
            //    }
            //}
            CenterMapOnPlayer();
        }

        private void CenterMapOnPlayer()
        {
            if (CenterOnPlayer)
            {
                Debug.WriteLine("C " + CenterRelativeToCanvas.ToString());
                var centerinworldspace = Transform.Inverse.Transform(CenterRelativeToCanvas);
                centerinworldspace.X += MapOffset.X;
                centerinworldspace.Y += MapOffset.Y;
                centerinworldspace.X *= -1;
                centerinworldspace.Y *= -1;
                // centerinworldspace = new Point(centerinworldspace.Y, centerinworldspace.X);
                Debug.WriteLine("C1 " + centerinworldspace.ToString());
                var loc = new Point(Lastlocation.X, Lastlocation.Y);
                var delta = Point.Subtract(loc, centerinworldspace);
                Debug.WriteLine("CD " + delta.ToString());
                var translate = new TranslateTransform(delta.X, delta.Y);
                Transform.Matrix = Transform.Matrix * translate.Value;
                foreach (UIElement child in Canvas.Children)
                {
                    if (child is ArrowLine c)
                    {
                        var transform = new MatrixTransform();
                        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = c.RotateTransform.Value * translation.Value;
                        c.RenderTransform = transform;
                    }
                    else if (child is Ellipse el)
                    {
                        var transform = new MatrixTransform();
                        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = translation.Value;
                        el.RenderTransform = transform;
                    }
                    else
                    {
                        child.RenderTransform = Transform;
                    }
                }
            }
        }
        public void UpdateTimerWidgest()
        {
            var removewidgets = new List<MapWidget>();
            foreach (var item in Canvas.Children)
            {
                if (item is MapWidget m)
                {
                    if (m.Update() <= -60 * 4)
                    {
                        removewidgets.Add(m);
                    }
                }
            }

            foreach (var item in removewidgets)
            {
                timersService.RemoveTimer(item.TimerInfo);
                Canvas.Children.Remove(item);
            }

            var playerstoremove = new List<PlayerLocation>();
            foreach (var item in Players.Where(a => a.Player != null))
            {
                if ((DateTime.UtcNow - item.Player.TimeStamp).TotalMinutes > 1)
                {
                    playerstoremove.Add(item);
                }
            }
            foreach (var item in playerstoremove)
            {
                Players.Remove(item);
                RemoveFromCanvas(item);
            }
        }

        public void MoveToPlayerLocation(MapWidget mw)
        {
            if (PlayerLocation == null)
            {
                return;
            }
            mw.TimerInfo.Location = new Point(Lastlocation.Y, Lastlocation.X);
            Canvas.SetLeft(mw, Canvas.GetLeft(PlayerLocation.ArrowLine));
            Canvas.SetTop(mw, Canvas.GetTop(PlayerLocation.ArrowLine));
            mw.RenderTransform = Transform;
        }

        private int deathcounter = 1;
        public MapWidget AddTimer(TimeSpan timer, string title, bool autoIncrementDuplicateNames)
        {
            var mousePosition = Transform.Inverse.Transform(_mouseuppoint);
            mousePosition.X += MapOffset.X;
            mousePosition.Y += MapOffset.Y;
            mousePosition.X *= -1;
            mousePosition.Y *= -1;
            if (autoIncrementDuplicateNames && timersService.TimerExists(ZoneName, title))
            {
                deathcounter = ++deathcounter > 999 ? 1 : deathcounter;
                title += "_" + deathcounter;
            }
            var mw = timersService.AddTimer(new TimerInfo
            {
                Duration = timer,
                Name = title,
                ZoneName = ZoneName,
                Fontsize = MapViewModelService.SmallFontSize(this.AABB),
                StartTime = DateTime.Now,
                Location = mousePosition
            });
            mw.Tag = title;
            _ = Canvas.Children.Add(mw);
            Canvas.SetTop(mw, _mouseuppoint.Y - Transform.Value.OffsetY);
            Canvas.SetLeft(mw, _mouseuppoint.X - Transform.Value.OffsetX);
            mw.RenderTransform = Transform;
            return mw;
        }

        public TimerInfo DeleteSelectedTimer()
        {
            if (_selectedElement is MapWidget w)
            {
                timersService.RemoveTimer(w.TimerInfo);
                Canvas.Children.Remove(w);
                _dragging = false;
                _selectedElement = null;
                return w.TimerInfo;
            }
            return null;
        }

        public void DeleteSelectedTimerByName(string name)
        {
            var timer = timersService.RemoveTimer(name);
            if (timer != null)
            {
                MapWidget wremove = null;
                foreach (var item in Canvas.Children)
                {
                    if (item is MapWidget w && w.TimerInfo.Name == name)
                    {
                        wremove = w;
                        break;
                    }
                }

                if (wremove != null)
                {
                    Canvas.Children.Remove(wremove);
                }
            }
        }

        public void PanAndZoomCanvas_MouseDown(Point mousePostion, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _selectedElement = e.Source is MapWidget ? (UIElement)e.Source : null;
            }
            if (TimerOpen)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.Source is MapWidget)
                {
                    _selectedElement = (UIElement)e.Source;
                    var x = Canvas.GetLeft(_selectedElement);
                    var y = Canvas.GetTop(_selectedElement);
                    var elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePostion;
                    _dragging = true;
                }
                else
                {
                    _dragging = false;
                    _selectedElement = null;
                }
            }

            if (!_dragging && e.ChangedButton == MouseButton.Left)
            {
                _initialMousePosition = Transform.Inverse.Transform(mousePostion);
            }
        }

        public void PanAndZoomCanvas_MouseUp(Point mousePostion)
        {
            _mouseuppoint = mousePostion;
            _dragging = false;
        }

        public void MoveMap(int x, int y)
        {
            var translate = new TranslateTransform(x, y);
            Transform.Matrix = translate.Value * Transform.Matrix;
            foreach (UIElement child in Canvas.Children)
            {
                if (child is ArrowLine c)
                {
                    var transform = new MatrixTransform();
                    var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                    transform.Matrix = c.RotateTransform.Value * translation.Value;
                    c.RenderTransform = transform;
                }
            }
        }

        public void PanAndZoomCanvas_MouseMove(Point mousePosition, MouseButtonState LeftButtonState)
        {
            //Debug.WriteLine(mousePosition.ToString());
            if (TimerOpen)
            {
                return;
            }

            if (!_dragging && LeftButtonState == MouseButtonState.Pressed)
            {
                var mousePosition1 = Transform.Inverse.Transform(mousePosition);
                var delta = Point.Subtract(mousePosition1, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                Transform.Matrix = Transform.Matrix * translate.Value;
                var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                EllipseTransform.Matrix = translation.Value;

                foreach (FrameworkElement child in Canvas.Children)
                {
                    if (child is ArrowLine c)
                    {
                        var transform = new MatrixTransform();
                        var arrowtranslation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = c.RotateTransform.Value * arrowtranslation.Value;
                        c.RenderTransform = transform;
                    }
                }
                UpdateAllPLayers();
            }

            if (_dragging && LeftButtonState == MouseButtonState.Pressed)
            {
                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, mousePosition.X + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement, mousePosition.Y + _draggingDelta.Y);
                    if (_selectedElement is MapWidget m)
                    {
                        var point = new Point(Canvas.GetLeft(_selectedElement), Canvas.GetTop(_selectedElement));
                        point = _selectedElement.RenderTransform.Inverse.Transform(point);
                        m.TimerInfo.Location = _selectedElement.RenderTransform.Inverse.Transform(point);
                    }
                }
            }
            mousePosition = Transform.Inverse.Transform(mousePosition);
            mousePosition.X += MapOffset.X;
            mousePosition.Y += MapOffset.Y;
            mousePosition.X *= -1;
            mousePosition.Y *= -1;
            LastMouselocation = mousePosition;
        }

        public void PanAndZoomCanvas_MouseWheel(Point mousePostion, int delta)
        {
            if (TimerOpen || _dragging)
            {
                return;
            }

            var scaleFactor = Zoomfactor;
            if (delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
                if (CurrentScaling == 1.0f)
                {
                    return;
                }
            }

            if (CurrentScaling * scaleFactor > 40)
            {
                return;
            }
            if (CurrentScaling * scaleFactor < 1 && CurrentScaling != 1.0f)
            {
                scaleFactor = 1.0f / CurrentScaling;
            }

            var scaleMatrix = Transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            Transform.Matrix = scaleMatrix;
            CurrentScaling *= scaleFactor;
            var currentlabelscaling = (CurrentScaling / 40 * -1) + 1;
            var zoneLabelFontSize = MapViewModelService.ZoneLabelFontSize(this.AABB);
            var otherLabelFontSize = MapViewModelService.OtherLabelFontSize(this.AABB);
            var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
            EllipseTransform.Matrix = translation.Value;
            foreach (FrameworkElement child in Canvas.Children)
            {
                if (child.Tag == null)
                {
                    continue;
                }
                var x = Canvas.GetLeft(child);
                var y = Canvas.GetTop(child);

                var sx = x * scaleFactor;
                var sy = y * scaleFactor;

                if (child is Ellipse el)
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                }
                else if (child is TextBlock t)
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                    var textdata = t.Tag as MapLabel;
                    if (textdata?.LabelSize == LabelSize.Large)
                    {
                        var largescaling = zoneLabelFontSize;
                        largescaling *= currentlabelscaling;
                        largescaling = (int)Clamp(largescaling, 5, 200);
                        if (t.FontSize != largescaling)
                        {
                            t.FontSize = largescaling;
                        }
                    }
                    else
                    {
                        var smallscaling = otherLabelFontSize;
                        smallscaling *= currentlabelscaling;
                        smallscaling = (int)Clamp(smallscaling, 5, 100);
                        if (t.FontSize != smallscaling)
                        {
                            t.FontSize = smallscaling;
                        }
                    }
                }
                else if (child is MapWidget)
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                    // child.RenderTransform = Transform;
                }
            }
            UpdateAllPLayers();
        }

        private void UpdateAllPLayers()
        {
            foreach (var item in Players.Where(a => a.Player != null))
            {
                MapViewModelService.UpdateLocation(new UpdateLocationData
                {
                    Trackingdistance = item.Player.TrackingDistance,
                    CurrentScaling = CurrentScaling,
                    MapOffset = MapOffset,
                    Oldlocation = new Point3D(item.Player.X, item.Player.Y, item.Player.Z),
                    Newlocation = new Point3D(item.Player.X, item.Player.Y, item.Player.Z),
                    PlayerLocationCircle = item,
                    Transform = Transform
                });
            }
            MapViewModelService.UpdateLocation(new UpdateLocationData
            {
                Trackingdistance = this.activePlayer?.Player?.TrackingDistance,
                CurrentScaling = CurrentScaling,
                MapOffset = MapOffset,
                Oldlocation = Lastlocation,
                Newlocation = Lastlocation,
                PlayerLocationCircle = PlayerLocation,
                Transform = Transform
            });
        }

        public void TimerMenu_Closed()
        {
            TimerOpen = false;
        }

        public void TimerMenu_Opened()
        {
            TimerOpen = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void PlayerLocationEvent(SignalrPlayer e)
        {
            e.TimeStamp = DateTime.UtcNow;
            var p = this.Players.FirstOrDefault(a => a.Player?.Name == e.Name);
            if (p == null)
            {
                var playerloc = AddPlayerToCanvas(e);
                this.Players.Add(playerloc);
            }
            else
            {
                MapViewModelService.UpdateLocation(new UpdateLocationData
                {
                    Trackingdistance = e.TrackingDistance,
                    CurrentScaling = CurrentScaling,
                    MapOffset = MapOffset,
                    Oldlocation = new Point3D(p.Player.X, p.Player.Y, p.Player.Z),
                    Newlocation = new Point3D(e.X, e.Y, e.Z),
                    PlayerLocationCircle = p,
                    Transform = Transform
                });
                p.Player = e;
            }
        }

        public void PlayerDisconnected(SignalrPlayer e)
        {
            var p = this.Players.FirstOrDefault(a => a.Player.Name == e.Name);
            if (p != null)
            {
                RemoveFromCanvas(p);
            }
        }
    }
}
