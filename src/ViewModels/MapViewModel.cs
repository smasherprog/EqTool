using EQTool.Services;
using EQTool.Services.Map;
using HelixToolkit.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace EQTool.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly ZoneParser zoneParser;

        public MapViewModel(ZoneParser zoneParser, MapLoad mapLoad, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.zoneParser = zoneParser;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
        }

        private ArrowVisual3D PlayerVisualLocation;
        private SphereVisual3D PlayerVisualLocationSphere;
        private Point3D? Lastlocation;

        public ObservableCollection<Visual3D> DrawItems { get; set; } = new ObservableCollection<Visual3D>();

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

        public bool LoadMap(string zone, Canvas canvas)
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
                canvas.Children.Clear();
                var min = map.AABB.Min;
                var max = map.AABB.Max;
                foreach (var group in map.Lines)
                {
                    var l = new Line
                    {
                        X1 = group.Points[0].X,
                        Y1 = group.Points[0].Y,
                        X2 = group.Points[1].X,
                        Y2 = group.Points[1].Y,
                        StrokeThickness = 2,
                        Stroke = new SolidColorBrush(group.Color)
                    };
                    _ = canvas.Children.Add(l);
                }

                canvas.Height = Math.Abs(map.AABB.MaxHeight);
                canvas.Width = Math.Abs(map.AABB.MaxWidth);
                Debug.WriteLine($"Labels: {map.Labels.Count}");
                foreach (var item in map.Labels)
                {
                    var text = new EQTextVisual3D
                    {
                        Text = item.label.Replace('_', ' '),
                        Position = item.Point,
                        Foreground = new SolidColorBrush(item.Color),
                        TextDirection = new Vector3D(1, 0, 0),
                        UpDirection = new Vector3D(0, 1, 0),
                        Height = 20
                    };
                    DrawItems.Add(text);
                }

                DrawItems.Add(new QuadVisual3D
                {
                    Point1 = new Point3D(max.X, max.Y, 0),
                    Point2 = new Point3D(max.X, min.Y, 0),
                    Point3 = new Point3D(min.X, min.Y, 0),
                    Point4 = new Point3D(min.X, max.Y, 0),
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 200, 1, 1))
                });
                PlayerVisualLocationSphere = new SphereVisual3D
                {
                    Radius = 2,
                    Fill = System.Windows.Media.Brushes.LimeGreen,
                    Center = new Point3D(0, 0, -1000)
                };
                PlayerVisualLocation = new ArrowVisual3D
                {
                    Direction = new Vector3D(1, 0, 0),
                    Point1 = new Point3D(0, 0, -1000),
                    Point2 = new Point3D(1000, 0, -1000),
                    Diameter = 1,
                    Fill = System.Windows.Media.Brushes.LimeGreen
                };
                return true;
            }

            return false;
        }

        public void Update()
        {
            appDispatcher.DispatchUI(() =>
            {
                //var maxdist = 100.0 * 100.0;
                //foreach (LinesVisual3D item in DrawItems.Where(a => a.GetType() == typeof(LinesVisual3D)))
                //{
                //    var dist = item.Points.First().DistanceToSquared(Position);
                //    var alpha = dist / maxdist;
                //    alpha = Math.Min(1, Math.Max(0, alpha));
                //    alpha = (alpha - 1) * -1;
                //    alpha *= 255;
                //    item.Color = Color.FromArgb(200, item.Color.R, item.Color.G, item.Color.B);
                //}
            });
        }
        public T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        public bool LoadDefaultMap(Canvas canvas)
        {
            _ = activePlayer.Update();
            var z = zoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            return LoadMap(z, canvas);
        }

        public void UpdateLocation(Point3D value1)
        {
            //var newval = new Point3D(value1.Y, value1.X, cameraDetail.Position.Z + 300);
            //if (!Lastlocation.HasValue)
            //{
            //    Lastlocation = new Point3D(value1.Y, value1.X, newval.Z);
            //}
            //var vec = newval - new Point3D(Lastlocation.Value.X, Lastlocation.Value.Y, cameraDetail.Position.Z + 300);
            //vec.Normalize();
            //var endpos = ((vec * 20) + newval.ToVector3D()).ToPoint3D();
            //Lastlocation = new Point3D(value1.Y, value1.X, newval.Z);


            //PlayerVisualLocationSphere.BeginEdit();
            //PlayerVisualLocationSphere.Center = newval;
            //PlayerVisualLocationSphere.EndEdit();

            //PlayerVisualLocation.BeginEdit();
            //PlayerVisualLocation.Direction = vec;
            //PlayerVisualLocation.Point1 = newval;
            //PlayerVisualLocation.Point2 = endpos;
            //PlayerVisualLocation.EndEdit();
            //return new CameraDetail
            //{
            //    LookDirection = cameraDetail.LookDirection,
            //    Position = new Point3D(value1.Y, value1.X, cameraDetail.Position.Z)
            //};
        }


        public void UpdatePlayerVisual(Point3D camera_position)
        {
            if (PlayerVisualLocation == null || PlayerVisualLocationSphere == null)
            {
                return;
            };

            PlayerVisualLocationSphere.BeginEdit();
            PlayerVisualLocationSphere.Center = new Point3D(PlayerVisualLocationSphere.Center.X, PlayerVisualLocationSphere.Center.Y, camera_position.Z + 300);
            PlayerVisualLocationSphere.EndEdit();
            PlayerVisualLocation.BeginEdit();
            PlayerVisualLocation.Direction = PlayerVisualLocation.Direction;
            PlayerVisualLocation.Point1 = new Point3D(PlayerVisualLocation.Point1.X, PlayerVisualLocation.Point1.Y, camera_position.Z + 300);
            PlayerVisualLocation.Point2 = new Point3D(PlayerVisualLocation.Point2.X, PlayerVisualLocation.Point2.Y, camera_position.Z + 300);
            PlayerVisualLocation.EndEdit();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
