using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var lines = new List<string>();
            var checkformanualmaps = System.IO.Directory.GetCurrentDirectory() + "/maps";
            if (System.IO.Directory.Exists(checkformanualmaps))
            {

                //foreach (var item in ZoneParser.Zones)
                //{
                //    var zonen = ZoneParser.TranslateToMapName(item);
                //    var files = Directory.GetFiles(checkformanualmaps, zonen + "*.txt").Where(a => !a.Contains("_2")).ToList();
                //    foreach (var cleanfile in files)
                //    {
                //        File.Copy(cleanfile, "C:\\Users\\smash\\source\\repos\\smasherprog\\EqTool\\src\\bin\\Debug\\maps\\cleanfiles\\" + Path.GetFileName(cleanfile), true);
                //    }
                //}

                var resourcenames = Directory.GetFiles(checkformanualmaps, zone + "*.txt").ToList();
                foreach (var item in resourcenames)
                {
                    using (var stream = new FileStream(item, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        var l = reader.ReadToEnd();
                        var splits = l.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        lines.AddRange(splits);
                    }
                }
            }

            if (!lines.Any())
            {
                var list = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();


                var resourcenames = list.Where(a => a.ToLower().StartsWith("eqtool.map_files." + zone)).ToList();
                foreach (var item in resourcenames)
                {
                    using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(item))
                    using (var reader = new StreamReader(stream))
                    {
                        var l = reader.ReadToEnd();
                        var splits = l.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        lines.AddRange(splits);
                    }
                }
            }

            return Parse(lines);
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
            public double MaxWidth => Max.X - Min.X;
        }

        public class ParsedData
        {
            public List<MapLine> Lines { get; set; } = new List<MapLine>();
            public List<MapLabel> Labels { get; set; } = new List<MapLabel>();
            public AABB AABB { get; set; } = new AABB();
            public Point3D Offset { get; set; }
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

        private ParsedData Parse(List<string> lines)
        {
            var ret = new ParsedData();
            var ls = lines.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            foreach (var item in ls)
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

            ret.AABB = new AABB();
            foreach (var point in ret.Lines)
            {
                ret.AABB.Add(point.Points[0]);
                ret.AABB.Add(point.Points[1]);
            }
            var min = ret.AABB.Min;

            foreach (var item in ret.Lines)
            {
                item.Points[0].X = item.Points[0].X - min.X;
                item.Points[0].Y = item.Points[0].Y - min.Y;
                item.Points[0].Z = item.Points[0].Z - min.Z;

                item.Points[1].X = item.Points[1].X - min.X;
                item.Points[1].Y = item.Points[1].Y - min.Y;
                item.Points[1].Z = item.Points[1].Z - min.Z;
            }
            ret.Offset = min;
            foreach (var item in ret.Labels)
            {
                item.Point = new Point3D
                {
                    X = item.Point.X - min.X,
                    Y = item.Point.Y - min.Y,
                    Z = item.Point.Z - min.Z
                };
            }

            ret.AABB = new AABB();
            foreach (var point in ret.Lines)
            {
                ret.AABB.Add(point.Points[0]);
                ret.AABB.Add(point.Points[1]);
            }
            var biggestdim = ret.AABB.MaxHeight > ret.AABB.MaxWidth ? ret.AABB.MaxHeight : ret.AABB.MaxWidth;
            Debug.WriteLine($"{biggestdim}");
            return ret;
        }
    }
}
