using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EQTool.Services
{
    public class MapLoad
    {
        public ParsedData Load(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                zone = "freportw";
            }

            var dir = System.IO.Directory.GetCurrentDirectory() + "/map_files/";
            var directory = new DirectoryInfo(dir);
            var f = directory.GetFiles(zone + "*").ToList();
            var lines = f.SelectMany(a => System.IO.File.ReadAllLines(a.FullName)).ToList();
            var maplines = Parse(lines);
            return maplines;
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

            public Point3D Center => new Point3D
            {
                X = .5 * (Min.X + Max.X),
                Y = .5 * (Min.Y + Max.Y),
                Z = .5 * (Min.Z + Max.Z)
            };

            public double MaxHeight => Max.Y - Min.Y;
        }

        public class ParsedData
        {
            public List<MapLine> Lines { get; set; } = new List<MapLine>();
            public List<MapLabel> Labels { get; set; } = new List<MapLabel>();
            public AABB AABB { get; set; } = new AABB();
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
                                 X = float.Parse(splits[0]) *-1,
                                 Y = float.Parse(splits[1]) *-1,
                                 Z = float.Parse(splits[2])
                             },
                             new Point3D
                             {
                                 X = float.Parse(splits[3]) *-1,
                                 Y = float.Parse(splits[4]) * -1,
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
                            X = float.Parse(splits[0]) * -1,
                            Y = float.Parse(splits[1]) * -1,
                            Z = float.Parse(splits[2])
                        },
                        Color = Color.FromRgb(byte.Parse(splits[3]), byte.Parse(splits[4]), byte.Parse(splits[5])),
                        label = splits[7]
                    });
                }
                foreach (var point in ret.Lines)
                {
                    ret.AABB.Add(point.Points[0]);
                    ret.AABB.Add(point.Points[1]);
                }
            }
            return ret;
        }
    }
}
