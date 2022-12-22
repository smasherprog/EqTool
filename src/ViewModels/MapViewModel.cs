using EQTool.Services;
using EQTool.Services.Map;
using HelixToolkit.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace EQTool.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly LogParser logParser;
        private readonly ZoneParser zoneParser;

        public MapViewModel(ZoneParser zoneParser, LogParser logParser, MapLoad mapLoad, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.zoneParser = zoneParser;
            this.logParser = logParser;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            Lastlocation = new Point3D(0, 0, 0);
           
            Camera = new OrthographicCamera
            {
                Width = 300,
                LookDirection = new Vector3D(0, 0, -100),
                Position = new Point3D(0, 1000, 100),
                UpDirection = new Vector3D(0, 1, 0)
            };
            this.LastLookDirection = new Vector3D(0, 0, -100);
            UpdateLocation(new Point3D(0, 0, 0));
        }

        public ProjectionCamera Camera { get; set; }
        private Vector3D LastLookDirection;
        private ArrowVisual3D Arrow;
        private Point3D Lastlocation;

        public ObservableCollection<Visual3D> DrawItems { get; set; } = new ObservableCollection<Visual3D>();

        private string _MapName = string.Empty;

        public string MapName
        {
            get => _MapName;
            set
            {
                _MapName = value;
                OnPropertyChanged();
            }
        }
        public void LoadMap(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                return;
            }
            Debug.WriteLine($"Loading: {zone}");
            var map = mapLoad.Load(zone);

            if (map.Labels.Any() || map.Lines.Any())
            {
                DrawItems.Clear();
                DrawItems.Add(new DefaultLights());
                var colorgroups = map.Lines.GroupBy(a => new { a.Color.R, a.Color.G, a.Color.B }).ToList();
                Debug.WriteLine($"LineGroups: {colorgroups.Count}");
                foreach (var group in colorgroups)
                {
                    var points = group.SelectMany(a => a.Points).ToList();
                    Debug.WriteLine($"Lines: {points.Count}");
                    var l = new HelixToolkit.Wpf.LinesVisual3D
                    {
                        Thickness = 1,
                        Points = new Point3DCollection(points),
                        Color = group.FirstOrDefault().Color
                    };
                    DrawItems.Add(l);
                }

                Debug.WriteLine($"Labels: {map.Labels.Count}");
                foreach (var item in map.Labels)
                {
                    var text = new TextVisual3D
                    {
                        Text = item.label,
                        Position = item.Point,
                        Foreground = new SolidColorBrush(item.Color),
                        TextDirection = new Vector3D(-1, 0, 0),
                        UpDirection = new Vector3D(0, 1, 0),
                        Height = 30
                    };
                    DrawItems.Add(text);
                }
                var center = map.AABB.Center;
                center.Z = map.AABB.MaxHeight;
                Camera.Position = center;
                Camera.LookAt(map.AABB.Center, 2000);
            }
        }

        public void Update()
        {
            appDispatcher.DispatchUI(() =>
            {
                if (LastLookDirection != Camera.LookDirection)
                {
                    //var cameralooknormal = LastLookDirection = LookDirection;
                    //var cameraup = new Vector3D(0, 1, 0);
                    //cameralooknormal.Normalize();
                    //cameraup.Normalize();
                    //var textdir = Vector3D.CrossProduct(cameraup, cameralooknormal);
                    //foreach (TextVisual3D item in DrawItems.Where(a=> a.GetType() == typeof(TextVisual3D)))
                    //{
                    //    item.TextDirection = textdir;
                    //    item.UpDirection = cameraup;
                    //}
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void LoadDefaultMap()
        {
            var z = zoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            LoadMap(z);
        }

        public void UpdateLocation(Point3D value)
        {
            appDispatcher.DispatchUI(() =>
            {
                var vec = value - Lastlocation;
                var endpos = (vec * 50).ToPoint3D();
                if (Arrow != null)
                {
                    DrawItems.Remove(Arrow);
                }
                Arrow = new ArrowVisual3D
                {
                    Direction = vec
                };
                Arrow.Direction.Normalize();
                Arrow.Point1 = value;
                Arrow.Point2 = endpos;
                Arrow.Diameter = 20;
                Arrow.Fill = System.Windows.Media.Brushes.Green;
                Lastlocation = value;
                DrawItems.Add(Arrow);
                Camera.LookAt(value, 2000);
            });
        }
    }
}
