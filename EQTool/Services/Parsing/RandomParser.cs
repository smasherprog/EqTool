using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class RandomParser : IEqLogParser
    {
        private readonly string RollMessage = "**A Magic Die is rolled by ";
        private readonly string RollMessage2nd = "**It could have been any number from 0 to ";
        private readonly string RollMessage3nd = "but this time it turned up a ";
        private string PlayerRollName = string.Empty;
        private DateTime RollTime = DateTime.MinValue;

        private readonly LogEvents logEvents;

        public RandomParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = Parse(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public RandomRollEvent Parse(string line, DateTime timestamp, int lineCounter)
        {
            if (line.StartsWith(RollMessage))
            {
                PlayerRollName = line.Substring(RollMessage.Length).TrimEnd('.');
                RollTime = DateTime.UtcNow;
            }
            else if (line.StartsWith(RollMessage2nd))
            {
                if ((DateTime.UtcNow - RollTime).TotalSeconds > 2)
                {
                    PlayerRollName = string.Empty;
                    RollTime = DateTime.MinValue;
                    return null;
                }
                var maxroll = line.Substring(RollMessage2nd.Length);
                var commaindex = maxroll.IndexOf(',');
                if (commaindex != -1)
                {
                    maxroll = maxroll.Substring(0, commaindex);
                }
                commaindex = line.IndexOf(RollMessage3nd);
                if (commaindex != -1)
                {
                    var roll = line.Substring(commaindex + RollMessage3nd.Length).TrimEnd('.');
                    if (int.TryParse(roll, out var rollint) && int.TryParse(maxroll, out var maxrollint))
                    {
                        return new RandomRollEvent
                        {
                            PlayerName = PlayerRollName,
                            MaxRoll = maxrollint,
                            Roll = rollint,
                            TimeStamp = timestamp,
                            LineCounter = lineCounter,
                            Line = line
                        };
                    }
                }
            }

            return null;
        }
    }
}
