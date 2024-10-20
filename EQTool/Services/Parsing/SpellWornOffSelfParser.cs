using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffSelfParser : IEqLogParseHandler
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        public SpellWornOffSelfParser(EQSpells spells, LogEvents logEvents)
        {
            this.spells = spells;
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = MatchWornOffSelfSpell(line);
            if (m.Any())
            {
                logEvents.Handle(new SpellWornOffSelfEvent { SpellNames = m, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public List<string> MatchWornOffSelfSpell(string message)
        {
            return spells.WornOffSpells.TryGetValue(message, out var pspells)
                ? pspells.Select(a => a.name).ToList()
                       : new List<string>();
        }
    }
}
