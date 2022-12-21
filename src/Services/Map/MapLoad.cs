using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Net.Mime.MediaTypeNames;

namespace EQTool.Services
{
    public class MapLoad
    {
        public class MapDetails
        {
            public List<HelixToolkit.Wpf.LinesVisual3D> Lines { get; set; } = new List<LinesVisual3D>();

            public List<TextVisual3D> Labels { get; set; } = new List<TextVisual3D>();

            public AABB AABB { get; set; } = new AABB();
        }

        public MapDetails Load(string zone)
        {
            var ret = new MapDetails();
            if (string.IsNullOrWhiteSpace(zone))
            {
                return ret;
            }

            var dir = System.IO.Directory.GetCurrentDirectory() + "/map_files/";
            var directory = new DirectoryInfo(dir);
            var f = directory.GetFiles().Where(a => a.Name.ToLower().StartsWith(zone)).ToList();
            var lines = f.SelectMany(a => System.IO.File.ReadAllLines(a.FullName)).ToList();
            var maplines = Parse(lines);
            var colorgroups = maplines.Lines.GroupBy(a => new { a.Color.R, a.Color.G, a.Color.B }).ToList();
 
            foreach (var group in colorgroups)
            {
                var points = group.SelectMany(a => a.Points).ToList();
                foreach (var point in points)
                {
                    ret.AABB.Add(point);
                }
                var l = new HelixToolkit.Wpf.LinesVisual3D();
                l.Thickness = 1;
                l.Points = new Point3DCollection(points);
                l.Color = group.FirstOrDefault().Color;
                ret.Lines.Add(l);
            }
            foreach (var item in maplines.Labels)
            {
                var text = new TextVisual3D();
                ret.AABB.Add(item.Point);
                text.Text = item.label;
                text.Position = item.Point;
                text.Foreground = new SolidColorBrush(item.Color);
                text.TextDirection = new Vector3D(1, 0, 0);
                text.UpDirection = new Vector3D(0, 1, 0);
                text.Height = 30;
                ret.Labels.Add(text);
            }

            return ret;
        }

        public class MapLine
        {
            public Point3D[] Points { get; set; }
            public Color Color { get; set; }
        }

        public class MapLabel
        {
            public Point3D Point { get; set; }
            public Color Color { get; set; }
            public string label { get; set; }
        }

        public class AABB
        {
            public Point3D Min = new Point3D { X = double.MaxValue, Y = double.MaxValue, Z = double.MaxValue };
            public Point3D Max = new Point3D { X = double.MinValue, Y = double.MinValue, Z = double.MinValue };

            public void Add(Point3D point)
            {
                Max.X = Math.Max(Max.X, point.X);
                Max.Y = Math.Max(Max.Y, point.Y);
                Max.Z = Math.Max(Max.Z, point.Z);
                Min.X = Math.Min(Min.X, point.X);
                Min.Y = Math.Min(Min.Y, point.Y);
                Min.Z = Math.Min(Min.Z, point.Z);
            }

            public Point3D Center
            {
                get
                {
                    return new Point3D
                    {
                        X = .5 * (Min.X + Max.X),
                        Y = .5 * (Min.Y + Max.Y),
                        Z = .5 * (Min.Z + Max.Z)
                    };
                }
            }

            public double MaxHeight 
            {
                get
                {
                    return  Max.Y - Min.Y;
                }
            }
        }

        public class ParsedData
        {
            public List<MapLine> Lines { get; set; } = new List<MapLine>();
            public List<MapLabel> Labels { get; set; } = new List<MapLabel>();
        }

        private ParsedData Parse(List<string> lines)
        {
            var ret = new ParsedData();

            foreach (var item in lines)
            {
                if (item.StartsWith("L "))
                {
                    // "L 4165.8896, 2586.66, -26.9688, 4165.4771, 2569.9077, -26.8348, 64, 64, 64"
                    var splits = item.Substring(2)
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .ToList();
                    ret.Lines.Add(new MapLine
                    {
                        Points = new Point3D[] {
                             new Point3D
                             {
                                 X = float.Parse(splits[0]),
                                 Y = float.Parse(splits[1]),
                                 Z = float.Parse(splits[2])
                             },
                             new Point3D
                             {
                                 X = float.Parse(splits[3]),
                                 Y = float.Parse(splits[4]),
                                 Z = float.Parse(splits[5])
                             }
                         },
                        Color = Color.FromRgb(byte.Parse(splits[6]), byte.Parse(splits[7]), byte.Parse(splits[8]))
                    });
                }
                else if (item.StartsWith("P "))
                {
                    //P -803.2998, 1038.5802, -15.833, 0, 0, 127, 2, Gellrazz_Scalerunner
                    var splits = item.Substring(2)
                       .Split(',')
                       .Select(s => s.Trim())
                       .Where(a => !string.IsNullOrWhiteSpace(a))
                       .ToList();
                    ret.Labels.Add(new MapLabel
                    {
                        Point = new Point3D
                        {
                            X = float.Parse(splits[0]),
                            Y = float.Parse(splits[1]),
                            Z = float.Parse(splits[2])
                        },
                        Color = Color.FromRgb(byte.Parse(splits[3]), byte.Parse(splits[4]), byte.Parse(splits[5])),
                        label = splits[7]
                    });
                }
            }
            return ret;
        }
    }
}
