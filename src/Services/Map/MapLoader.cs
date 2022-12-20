using EQTool.Models;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace EQTool.Services
{
    public class MapLoader
    {
        public class ShapeItem
        {
            public Geometry Geometry { get; set; }
            public Brush Stroke { get; set; }
        }

        private readonly EQToolSettings settings;

        public MapLoader(EQToolSettings settings)
        {
            this.settings = settings;
        }

        public List<ShapeItem> Load()
        {
            var lines = new List<ShapeItem>();
            _ = File.ReadAllLines(settings.DefaultEqDirectory + "/map_files/blackburrow.txt");
            //foreach (var item in text)
            //{
            //    if (item.StartsWith("L"))
            //    {
            //        var pos = item.TrimStart('L');
            //        var locs = pos.Split(',').Select(a => double.Parse(a.Trim())).ToArray();
            //        var p1 = new Point(100, 100)
            //          var myPolygon = new Polygon
            //          {
            //              Stroke = System.Windows.Media.Brushes.Black,
            //              Fill = System.Windows.Media.Brushes.LightSeaGreen,
            //              StrokeThickness = 2,
            //              HorizontalAlignment = HorizontalAlignment.Left,
            //              VerticalAlignment = VerticalAlignment.Center
            //          };
            //        var Point1 = new System.Windows.Point(1, 50);
            //        var Point2 = new System.Windows.Point(10, 80);
            //        var Point3 = new System.Windows.Point(50, 50);
            //        var myPointCollection = new PointCollection
            //        {
            //            Point1,
            //            Point2,
            //            Point3
            //        };
            //        myPolygon.Points = myPointCollection;
            //        myPolygon.
            //        var line = new ShapeItem
            //        {
            //            Geometry = new LineGeometry(new System.Windows.Point)
            //            X1 = locs[0],
            //            Y1 = locs[1],
            //            z
            //        }
            //    }
            //}

            return lines;
        }
    }
}
