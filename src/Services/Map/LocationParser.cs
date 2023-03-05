using System.Globalization;
using System.Linq;
using System.Windows.Media.Media3D;

namespace EQTool.Services.Map
{
    public class LocationParser
    {
        private readonly string YourLocationis = "Your Location is ";
        public Point3D? Match(string message)
        {
            //Debug.WriteLine($"LocationParser: " + message);
            if (message.StartsWith(YourLocationis))
            {
                var pos = message.Replace(YourLocationis, string.Empty)
                    .Trim()
                    .Split(',')
                    .Select(s => float.Parse(s.Trim(), CultureInfo.InvariantCulture))
                    .ToList();
                var ret = new Point3D { X = pos[0], Y = pos[1], Z = pos[2] };
                //Debug.WriteLine($"Pos x={ret.X}, y={ret.Y}, z={ret.Z}");
                return ret;
            }

            return null;
        }

    }
}
