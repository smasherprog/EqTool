using EQToolShared.Map;
using System;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace EQTool.Services
{
    public static class MapOpacityHelper
    {
        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        public static void SetOpacity(Shape l, double v)
        {
            if (l.Stroke.IsFrozen)
            {
                l.Stroke = l.Stroke.Clone();
            }

            l.Stroke.Opacity = v;
        }

        public static void SetOpacity(TextBlock l, double v)
        {
            if (l.Foreground.IsFrozen)
            {
                l.Foreground = l.Foreground.Clone();
            }

            l.Foreground.Opacity = v;
        }

        public static void AdjustOpacity(double shortestdistance, Shape shape, ZoneInfo zoneinfo)
        {
            var twiceheight = zoneinfo.ZoneLevelHeight * 2;
            if (shortestdistance < zoneinfo.ZoneLevelHeight)
            {
                SetOpacity(shape, 1);
            }
            else if (shortestdistance >= zoneinfo.ZoneLevelHeight && shortestdistance <= twiceheight + zoneinfo.ZoneLevelHeight)
            {
                var dist = ((shortestdistance - zoneinfo.ZoneLevelHeight) * -1) + twiceheight; //changed range to [0,80] with 0 being the FURTHest distance
                dist = (dist / twiceheight) + .1; // scale to [.1,1.1] 
                _ = Clamp(dist, .1, 1);
                SetOpacity(shape, dist);
            }
            else
            {
                SetOpacity(shape, .1);
            }
        }

        public static void AdjustOpacity(TextBlock t, ZoneInfo zoneinfo, Point3D lastloc)
        {
            var twiceheight = zoneinfo.ZoneLevelHeight * 2;
            var m = t.Tag as MapLoad.MapLabel;
            if (m == null)
            {
                return;
            }
            var shortestdistance = Math.Abs(m.Point.Z - lastloc.Z);

            if (shortestdistance < zoneinfo.ZoneLevelHeight)
            {
                SetOpacity(t, 1);
            }
            else if (shortestdistance >= zoneinfo.ZoneLevelHeight && shortestdistance <= twiceheight + zoneinfo.ZoneLevelHeight)
            {
                var dist = ((shortestdistance - zoneinfo.ZoneLevelHeight) * -1) + twiceheight; //changed range to [0,80] with 0 being the FURTHest distance
                dist = (dist / twiceheight) + .1; // scale to [.1,1.1] 
                _ = Clamp(dist, .1, 1);
                SetOpacity(t, dist);
            }
            else
            {
                SetOpacity(t, .1);
            }
        }
    }
}
