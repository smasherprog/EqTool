using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class BeginsToCastTheGateSpellParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public BeginsToCastTheGateSpellParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = GateMatch(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new NPCBeginsToGateEvent { NPCName = m, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private string GateMatch(string message)
        {
            if (message.EndsWith("begins to cast the gate spell."))
            {
                return message.Replace("begins to cast the gate spell.", string.Empty).Trim();
            }
            return string.Empty;
        }
    }
}
