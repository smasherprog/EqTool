using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Media3D;

namespace EQTool.Services.Map
{
    public class LocationParser
    {
        private readonly string YourLocationis = "Your Location is ";
        public Point3D? Match(string linelog)
        {
            if (linelog == null || linelog.Length < 27)
            {
                return null;
            }
            //[Mon Sep 26 06:43:43 2022] Your Location is 971.48, -2875.58, 18.35
            var message = linelog.Substring(27);
            Debug.WriteLine($"ZoneParse: " + message);
            if (message.StartsWith(YourLocationis))
            {
                var pos = message.Replace(YourLocationis, string.Empty)
                    .Trim()
                    .Split(',')
                    .Select(s => float.Parse(s.Trim()))
                    .ToList();
                var ret = new Point3D { X = pos[0], Y = pos[1], Z = pos[2] };
                Debug.WriteLine($"Pos x={ret.X}, y={ret.Y}, z={ret.Z}");
                return ret;
            }

            return null;
        }

    }
}
