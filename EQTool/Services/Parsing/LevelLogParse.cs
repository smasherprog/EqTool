using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class LevelLogParse : IEqLogParseHandler
    {
        private readonly string YouHaveGainedALevel = "You have gained a level! Welcome to level";
        private readonly LogEvents logEvents;

        public LevelLogParse(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = MatchLevel(line);
            if (m.HasValue)
            {
                logEvents.Handle(new LevelEvent { NewLevel = m.Value, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public int? MatchLevel(string message)
        {
            if (message.StartsWith(YouHaveGainedALevel))
            {
                var levelstring = message.Replace(YouHaveGainedALevel, string.Empty).Trim().TrimEnd('!');
                if (int.TryParse(levelstring, out var level))
                {
                    return level;
                }
            }
            return null;
        }
    }
}
