using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Media3D;
using static EQTool.Services.EventsList;

namespace EQTool.Services.Map
{
    public class LocationParser : ILogParser
    {
        private readonly string YourLocationis = "Your Location is ";

        private readonly EventsList eventsList;
        private readonly ActivePlayer activePlayer;

        public LocationParser(EventsList eventsList, ActivePlayer activePlayer)
        {
            this.eventsList = eventsList;
            this.activePlayer = activePlayer;
        }

        public bool Evaluate(string line)
        {
            if (line.StartsWith(YourLocationis))
            {
                var pos = line.Replace(YourLocationis, string.Empty)
                    .Trim()
                    .Split(',')
                    .Select(s => float.Parse(s.Trim(), CultureInfo.InvariantCulture))
                    .ToList();
                var ret = new Point3D { X = pos[0], Y = pos[1], Z = pos[2] };
                this.eventsList.Handle(new PlayerLocationEventArgs { Location = ret, PlayerInfo = this.activePlayer?.Player });
                return true;
            }

            return false;
        }
    }
}
