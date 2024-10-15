using EQTool.Models;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class ResistSpellParser : IEqLogParseHandler
    {
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;

        public ResistSpellParser(EQSpells spells, LogEvents logEvents)
        {
            this.spells = spells;
            this.logEvents = logEvents;
        }


        public bool Handle(string line, DateTime timestamp)
        {
            var m = ParseNPCSpell(line);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public ResistSpellEvent ParseNPCSpell(string line)
        {
            var resistmessage = line.StartsWith("You resist the ");
            if (resistmessage)
            {
                var spellname = line.Replace("You resist the ", string.Empty).Replace(" spell!", string.Empty).Trim();
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    return new ResistSpellEvent { Spell = spell, isYou = true };
                }
            }

            resistmessage = line.StartsWith("Your target resisted the ");
            if (resistmessage)
            {
                var spellname = line.Replace("Your target resisted the ", string.Empty).Replace(" spell.", string.Empty).Trim();
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    return new ResistSpellEvent { Spell = spell, isYou = false };
                }
            }

            return null;
        }
    }
}
