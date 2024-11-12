using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Media3D;

namespace EQTool.Services.Parsing
{
    public class LocationParser : IEqLogParser
    {
        private readonly string YourLocationis = "Your Location is ";

        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;

        public LocationParser(LogEvents logEvents, ActivePlayer activePlayer)
        {
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = Match(line);
            if (m != null)
            {
                logEvents.Handle(new PlayerLocationEvent
                {
                    Location = m.Value,
                    PlayerInfo = activePlayer.Player,
                    TimeStamp = timestamp,
                    Line = line,
                    LineCounter = lineCounter
                });
                return true;
            }
            return false;
        }

        private Point3D? Match(string message)
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
