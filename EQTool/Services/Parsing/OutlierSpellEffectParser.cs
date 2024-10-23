using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class OutlierSpellEffectParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public OutlierSpellEffectParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "The screams fade away.")
            {
                logEvents.Handle(new SpellWornOffEvent { SpellName = "Soul Consumption", TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
