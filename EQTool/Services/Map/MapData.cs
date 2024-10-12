using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace EQTool.Services.Map
{

    [Serializable]
    public class MapLine
    {
        public Point3D[] Points { get; set; }

        public Colour Color { get; set; }
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

}
