using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class CharmBreakParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public CharmBreakParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool DidCharmBreak(string line)
        {
            return line == "Your charm spell has worn off.";
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = DidCharmBreak(line);
            if (m)
            {
                logEvents.Handle(new CharmBreakEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line });
                return true;
            }
            return false;
        }
    }
}
