using EQTool.Services;
using EQTool.Services.Map;
using HelixToolkit.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

        private Vector3D _LookDirection;

        public Vector3D LookDirection
        {
            get => _LookDirection;
            set
            {
                _LookDirection = value;
                OnPropertyChanged();
            }
        }

        private Point3D _Position;

        public Point3D Position
        {
            get => _Position;
            set
            {
                _Position = value;
                OnPropertyChanged();
            }
        }

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
                Debug.WriteLine($"MouseWorldCoordinates: {MouseWorldCoordinates}");
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public bool LoadMap(string zone)
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
                DrawItems.Clear();

                var colorgroups = map.Lines.GroupBy(a => new { a.Color.R, a.Color.G, a.Color.B }).ToList();
                Debug.WriteLine($"LineGroups: {colorgroups.Count}");
                foreach (var group in colorgroups)
                {
                    var points = group.SelectMany(a => a.Points).ToList();
                    var modifiedpoints = points.Select(a => new Point3D
                    {
                        X = a.X,
                        Y = a.Y,
                        Z = 0
                    }).ToList();
                    Debug.WriteLine($"Lines: {points.Count}");
                    var l = new EQLinesVisual3D
                    {
                        Thickness = 1,
                        Points = new Point3DCollection(modifiedpoints),
                        Color = group.FirstOrDefault().Color,
                        OriginalPoints = points
                    };
                    DrawItems.Add(l);
                }

                Debug.WriteLine($"Labels: {map.Labels.Count}");
                foreach (var item in map.Labels)
                {
                    var text = new EQTextVisual3D
                    {
                        Text = item.label,
                        Position = item.Point,
                        Foreground = new SolidColorBrush(item.Color),
                        TextDirection = new Vector3D(1, 0, 0),
                        UpDirection = new Vector3D(0, 1, 0),
                        Height = 30,
                    };
                    DrawItems.Add(text);
                }
                var halfbox = map.AABB.MaxHeight * .3;
                var center = map.AABB.Center;
                center.Z -= halfbox;
                Position = center;
                map.AABB.Min.X = map.AABB.Min.X - halfbox;
                map.AABB.Min.Y = map.AABB.Min.Y - halfbox;

                map.AABB.Max.X = map.AABB.Max.X + halfbox;
                map.AABB.Max.Y = map.AABB.Max.Y + halfbox;

                var min = map.AABB.Min;
                var max = map.AABB.Max;
                DrawItems.Add(new QuadVisual3D
                {
                    Point1 = new Point3D(max.X, max.Y, 0),
                    Point2 = new Point3D(max.X, min.Y, 0),
                    Point3 = new Point3D(min.X, min.Y, 0),
                    Point4 = new Point3D(min.X, max.Y, 0),
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 200, 1, 1))
                });
                LookDirection = map.AABB.Center - Position;
                DrawItems.Add(new DefaultLights());
            }

            return true;
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

        public void LoadDefaultMap()
        {
            var z = zoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            _ = LoadMap(z);
        }

        public void UpdateLocation(Point3D value1, Point3D camera_position)
        {
            appDispatcher.DispatchUI(() =>
            {
                var newval = new Point3D(value1.Y, value1.X, -40);
                if (!Lastlocation.HasValue)
                {
                    Lastlocation = new Point3D(value1.Y, value1.X, newval.Z);
                }
                var vec = newval - new Point3D(Lastlocation.Value.X, Lastlocation.Value.Y, -40);
                vec.Normalize();
                var endpos = ((vec * 30) + newval.ToVector3D()).ToPoint3D();
                Lastlocation = new Point3D(value1.Y, value1.X, newval.Z);
                _ = DrawItems.Remove(PlayerVisualLocation);
                _ = DrawItems.Remove(PlayerVisualLocationSphere);

                PlayerVisualLocationSphere = new SphereVisual3D
                {
                    Radius = 8,
                    Fill = System.Windows.Media.Brushes.LimeGreen,
                    Center = newval
                };

                PlayerVisualLocation = new ArrowVisual3D
                {
                    Direction = vec,
                    Point1 = newval,
                    Point2 = endpos,
                    Diameter = 5,
                    Fill = System.Windows.Media.Brushes.LimeGreen
                };

                DrawItems.Add(PlayerVisualLocation);
                DrawItems.Add(PlayerVisualLocationSphere);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
