using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Media3D;

namespace EQTool.Services
{
    public class MapLoad
    {
        private readonly LoggingService loggingService;
        public MapLoad(LoggingService loggingService)
        {
            this.loggingService = loggingService;
        }
        public ParsedData Load(string zone)
        {
            var stop = new Stopwatch();
            stop.Start();
            if (string.IsNullOrWhiteSpace(zone))
            {
                zone = "freportw";
            }
            var lines = new List<string>();
            var checkformanualmaps = System.IO.Directory.GetCurrentDirectory() + "/maps";
            var isdebug = false;
#if DEBUG
            //isdebug = true;
#endif
            if (isdebug && System.IO.Directory.Exists(checkformanualmaps))
            {
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
            var oldcachedmaps = Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory(), "cachedmaps*");
            var version = "cachedmaps" + App.Version.Replace(".", string.Empty).Trim();
            foreach (var item in oldcachedmaps)
            {
                if (!item.Contains(version))
                {
                    try
                    {
                        Directory.Delete(item, true);
                    }
                    catch
                    {
                        Thread.Sleep(2000);
                        Directory.Delete(item, true);
                    }
                }
            }
            checkformanualmaps = System.IO.Directory.GetCurrentDirectory() + $"/{version}";
            if (System.IO.File.Exists(checkformanualmaps + "/" + zone + ".bin"))
            {
                try
                {
                    var data = BinarySerializer.ReadFromBinaryFile<ParsedData>(checkformanualmaps + "/" + zone + ".bin");
                    stop.Stop();
                    Debug.WriteLine($"Time to load map from Cache {zone} {stop.ElapsedMilliseconds}");
                    return data;
                }
                catch (Exception ex)
                {
                    loggingService.Log(ex.ToString(), App.EventType.Error);
                }
            }
            try
            {
                _ = Directory.CreateDirectory(checkformanualmaps);
            }
            catch { }

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

            var d = Parse(lines);
            stop.Stop();
            Debug.WriteLine($"Time to load map {zone} {stop.ElapsedMilliseconds}");
            try
            {

                BinarySerializer.WriteToBinaryFile(checkformanualmaps + "/" + zone + ".bin", d);
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
            return d;
        }

        [Serializable]
        public class MapLine
        {
            public Point3D[] Points { get; set; }

            public Colour Color { get; set; }

            public EQMapColor ThemeColors { get; set; }
        }

        public enum LabelSize
        {
            Small,
            Medium,
            Large
        }

        [Serializable]
        public class MapLabel
        {
            public Point3D Point { get; set; }
            public Colour Color { get; set; }
            public string label { get; set; }
            public LabelSize LabelSize { get; set; }
        }

        [Serializable]
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

            public double MaxHeight => Max.Y == double.MinValue || Min.Y == double.MaxValue ? 600 : Max.Y - Min.Y;
            public double MaxWidth => Max.X == double.MinValue || Min.X == double.MaxValue ? 600 : Max.X - Min.X;
        }

        [Serializable]
        public class ParsedData
        {
            public List<MapLine> Lines { get; set; } = new List<MapLine>();
            public List<MapLabel> Labels { get; set; } = new List<MapLabel>();
            public AABB AABB { get; set; } = new AABB();
            public Point3D Offset { get; set; }
        }

        public static MapLabel CreateFromString(string[] splits)
        {
            return new MapLabel
            {
                Point = new Point3D
                {
                    X = float.Parse(splits[0], CultureInfo.InvariantCulture),
                    Y = float.Parse(splits[1], CultureInfo.InvariantCulture),
                    Z = float.Parse(splits[2], CultureInfo.InvariantCulture)
                },
                Color = System.Windows.Media.Color.FromRgb(byte.Parse(splits[3], CultureInfo.InvariantCulture), byte.Parse(splits[4], CultureInfo.InvariantCulture), byte.Parse(splits[5], CultureInfo.InvariantCulture)),
                LabelSize = splits[7].StartsWith("to_", StringComparison.OrdinalIgnoreCase) ? LabelSize.Large : LabelSize.Small,
                label = splits[7]
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
                                 X = float.Parse(splits[0], CultureInfo.InvariantCulture),
                                 Y = float.Parse(splits[1], CultureInfo.InvariantCulture),
                                 Z = float.Parse(splits[2], CultureInfo.InvariantCulture)
                             },
                             new Point3D
                             {
                                 X = float.Parse(splits[3], CultureInfo.InvariantCulture),
                                 Y = float.Parse(splits[4], CultureInfo.InvariantCulture),
                                 Z = float.Parse(splits[5], CultureInfo.InvariantCulture)
                             }
                         },
                        Color = System.Windows.Media.Color.FromRgb(byte.Parse(splits[6]), byte.Parse(splits[7]), byte.Parse(splits[8]))
                    });
                }
                else if (item.StartsWith("P "))
                {
                    var splits = item.Substring(2)
                      .Split(',')
                      .Select(s => s.Trim())
                      .Where(a => !string.IsNullOrWhiteSpace(a))
                      .ToArray();
                    if (splits.Length <= 7)
                    {
                        continue;
                    }
                    try
                    {
                        ret.Labels.Add(CreateFromString(splits));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(item + "    " + ex.ToString());
                    }
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
                item.Points[0].Z = item.Points[0].Z;

                item.Points[1].X = item.Points[1].X - min.X;
                item.Points[1].Y = item.Points[1].Y - min.Y;
                item.Points[1].Z = item.Points[1].Z;
            }
            ret.Offset = min;
            foreach (var item in ret.Labels)
            {
                item.Point = new Point3D
                {
                    X = item.Point.X - min.X,
                    Y = item.Point.Y - min.Y,
                    Z = item.Point.Z
                };
            }

            ret.AABB = new AABB();
            foreach (var point in ret.Lines)
            {
                ret.AABB.Add(point.Points[0]);
                ret.AABB.Add(point.Points[1]);
            }
            var biggestdim = ret.AABB.MaxHeight > ret.AABB.MaxWidth ? ret.AABB.MaxHeight : ret.AABB.MaxWidth;
            return ret;
        }
    }
}
