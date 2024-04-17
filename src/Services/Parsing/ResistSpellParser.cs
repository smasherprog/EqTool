using EQTool.Models;
using EQTool.Services.Parsing;
using System.Linq;

namespace EQTool.Services
{
    public class ResistSpellParser : ILogParser
    {
        private readonly EQSpells spells;
        private readonly EventsList eventsList;
        public ResistSpellParser(EQSpells spells, EventsList eventsList)
        {
            this.spells = spells;
            this.eventsList = eventsList;
        }
        public class ResistSpellData
        {
            public Spell Spell { get; set; }
            public bool isYou { get; set; }
        }

        public bool Evaluate(string line)
        {
            var resistmessage = line.StartsWith("You resist the ");
            if (resistmessage)
            {
                var spellname = line.Replace("You resist the ", string.Empty).Replace(" spell!", string.Empty).Trim();
                var spell = this.spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    this.eventsList.Handle(new ResistSpellData { Spell = spell, isYou = true });
                    return true;
                }
            }

            resistmessage = line.StartsWith("Your target resisted the ");
            if (resistmessage)
            {
                var spellname = line.Replace("Your target resisted the ", string.Empty).Replace(" spell.", string.Empty).Trim();
                var spell = this.spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    this.eventsList.Handle(new ResistSpellData { Spell = spell, isYou = false });
                    return true;
                }
            }

            return false;
        }
    }
}
