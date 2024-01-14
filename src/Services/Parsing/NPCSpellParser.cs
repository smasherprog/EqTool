using EQTool.Models;
using System.Linq;

namespace EQTool.Services
{
    public class NPCSpellParser
    {
        private readonly EQSpells spells;

        public NPCSpellParser(EQSpells spells)
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

            if (this.spells.CastOnYouSpells.TryGetValue(line, out var matches))
            {
                if (matches.Count == 1)
                {
                    return matches.FirstOrDefault();
                }
            }
            var firstspace = line.IndexOf("'s");
            if (firstspace != -1)
            {
                var innermsg = line.Substring(firstspace).Trim();
                if (this.spells.CastOtherSpells.TryGetValue(innermsg, out matches))
                {
                    if (matches.Count == 1)
                    {
                        return matches.FirstOrDefault();
                    }
                }
            }


            firstspace = line.IndexOf(" ");
            if (firstspace == -1)
            {
                return null;
            }

            var msg = line.Substring(firstspace).Trim();
            if (this.spells.CastOtherSpells.TryGetValue(msg, out matches))
            {
                if (matches.Count == 1)
                {
                    return matches.FirstOrDefault();
                }
            }
            return null;
        }
    }
}
