using EQTool.Models;
using System.Linq;

namespace EQTool.Services
{
    public class ResistSpellParser
    {
        private readonly EQSpells spells;

        public ResistSpellParser(EQSpells spells)
        {
            this.spells = spells;
        }

        public Spell ParseNPCSpell(string line)
        {
            var resistmessage = line.StartsWith("You resist the ");
            if (resistmessage)
            {
                var spellname = line.Replace("You resist the ", string.Empty).Replace(" spell!", string.Empty).Trim();
                var spell = this.spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                if (spell != null)
                {
                    return spell;
                }
            }

            return null;
        }
    }
}
