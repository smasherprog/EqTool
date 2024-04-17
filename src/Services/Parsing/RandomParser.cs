using EQTool.Services.Parsing;
using System;

namespace EQTool.Services
{
    public class RandomParser : ILogParser
    {
        public class RandomRollData
        {
            public string PlayerName { get; set; }

            public int MaxRoll { get; set; }

            public int Roll { get; set; }
        }

        private string RollMessage = "**A Magic Die is rolled by ";
        private string RollMessage2nd = "**It could have been any number from 0 to ";
        private string RollMessage3nd = "but this time it turned up a ";
        private string PlayerRollName = string.Empty;
        private DateTime RollTime = DateTime.MinValue;
        private readonly EventsList eventsList;

        public RandomParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line.StartsWith(RollMessage))
            {
                PlayerRollName = line.Substring(RollMessage.Length).TrimEnd('.');
                RollTime = DateTime.UtcNow;
                return true;
            }
            else if (line.StartsWith(RollMessage2nd))
            {
                if ((DateTime.UtcNow - RollTime).TotalSeconds > 2)
                {
                    PlayerRollName = string.Empty;
                    RollTime = DateTime.MinValue;
                    return true;
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
                        this.eventsList.Handle(new EventsList.RandomRollEventArgs
                        {
                            RandomRollData = new RandomRollData
                            {
                                PlayerName = PlayerRollName,
                                MaxRoll = maxrollint,
                                Roll = rollint
                            }
                        });
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
