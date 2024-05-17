using EQTool.Models;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class ResistSpellParser
    {
        private readonly EQSpells spells;

        public ResistSpellParser(EQSpells spells)
        {
            this.spells = spells;
        }
        public class ResistSpellData
        {
            public Spell Spell { get; set; }
            public bool isYou { get; set; }
        }

        public ResistSpellData ParseNPCSpell(string line)
        {
            var resistmessage = line.StartsWith("You resist the ");
            if (resistmessage)
            {
                var spellname = line.Replace("You resist the ", string.Empty).Replace(" spell!", string.Empty).Trim();
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    return new ResistSpellData { Spell = spell, isYou = true };
                }
            }

            resistmessage = line.StartsWith("Your target resisted the ");
            if (resistmessage)
            {
                var spellname = line.Replace("Your target resisted the ", string.Empty).Replace(" spell.", string.Empty).Trim();
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    return new ResistSpellData { Spell = spell, isYou = false };
                }
            }

            return null;
        }
    }
}
