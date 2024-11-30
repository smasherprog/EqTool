using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffSelfParser : IEqLogParser
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public SpellWornOffSelfParser(EQSpells spells, LogEvents logEvents, DebugOutput debugOutput)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = MatchWornOffSelfSpell(line);
            if (m.Any())
            {
                var spellsfound = string.Join(",", m);
                debugOutput.WriteLine($"{spellsfound} Message: {line}", OutputType.Spells);
                logEvents.Handle(new SpellWornOffSelfEvent { SpellNames = m, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
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
