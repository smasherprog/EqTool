using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffOtherParser : IEqLogParser
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        public SpellWornOffOtherParser(EQSpells spells, LogEvents logEvents)
        {
            this.spells = spells;
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = MatchWornOffOtherSpell(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
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
