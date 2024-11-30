using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffOtherParser : IEqLogParser
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public SpellWornOffOtherParser(EQSpells spells, LogEvents logEvents, DebugOutput debugOutput)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = MatchWornOffOtherSpell(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                debugOutput.WriteLine($"{m} Message: {line}", OutputType.Spells);
                logEvents.Handle(new SpellWornOffOtherEvent { SpellName = m, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private string MatchWornOffOtherSpell(string message)
        {
            return message.StartsWith(EQSpells.Your) && message.EndsWith(EQSpells.SpellHasWornoff)
                       ? message.Replace(EQSpells.Your, string.Empty).Replace(EQSpells.SpellHasWornoff, string.Empty).Trim()
                       : string.Empty;
        }
    }
}
