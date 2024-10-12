using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffOtherParser : IEqLogParseHandler
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        public SpellWornOffOtherParser(EQSpells spells, LogEvents logEvents)
        {
            this.spells = spells;
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = MatchWornOffOtherSpell(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new SpellWornOffOtherEvent { SpellName = m });
                return true;
            }
            return false;
        }


        public string MatchWornOffOtherSpell(string message)
        {
            return message.StartsWith(EQSpells.Your) && message.EndsWith(EQSpells.SpellHasWornoff)
                       ? message.Replace(EQSpells.Your, string.Empty).Replace(EQSpells.SpellHasWornoff, string.Empty).Trim()
                       : string.Empty;
        }
    }
}
