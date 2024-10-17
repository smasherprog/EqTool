using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class LevParser : IEqLogParseHandler
    {
        public enum LevStatus
        {
            Fading,
            Faded,
            Applied
        }
        private readonly LogEvents logEvents;

        public LevParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = Parse(line);
            if (m != null)
            {
                logEvents.Handle(new LevitateEvent { LevitateStatus = m.Value, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public LevStatus? Parse(string line)
        {
            return line == "You feel as if you are about to fall." ? LevStatus.Fading : (LevStatus?)null;
        }
    }
}
