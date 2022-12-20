using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace EQTool.Services
{
    public class MapLoad
    {
        public List<HelixToolkit.Wpf.LinesVisual3D> Load(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                return null;
            }

            var l = new HelixToolkit.Wpf.LinesVisual3D();
            l.Thickness = 1;

            var dir = System.IO.Directory.GetCurrentDirectory() + "/map_files/";
            var directory = new DirectoryInfo(dir);
            var f = directory.GetFiles()
                .Where(a => a.Name.ToLower().StartsWith(zone))
                .FirstOrDefault();
            var lines = System.IO.File.ReadAllLines(f.FullName);
            var points = new List<Point3D>();
            // "L 4165.8896, 2586.66, -26.9688, 4165.4771, 2569.9077, -26.8348, 64, 64, 64\r\n"
            foreach (var item in lines)
            {
                if (item.StartsWith("L "))
                {
                    var splits = item.Substring(2)
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .ToList();
                    points.Add(new Point3D
                    {
                        X = float.Parse(splits[0]),
                        Y = float.Parse(splits[1]),
                        Z = float.Parse(splits[2])
                    });

                    points.Add(new Point3D
                    {
                        X = float.Parse(splits[3]),
                        Y = float.Parse(splits[4]),
                        Z = float.Parse(splits[5])
                    });
                }
            }
            l.Points = new Point3DCollection(points);
            return l;

        }
    }
}
