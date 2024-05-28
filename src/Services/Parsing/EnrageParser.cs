using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class EnrageParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public EnrageParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = EnrageCheck(line);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }
        public EnrageEvent EnrageCheck(string line)
        {
            if (line.EndsWith(" has become ENRAGED.", System.StringComparison.OrdinalIgnoreCase))
            {
                var npcname = line.Replace(" has become ENRAGED.", string.Empty).Trim();
                return new EnrageEvent { NpcName = npcname };
            }

            return null;
        }
    }
}
