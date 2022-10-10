using EQTool.Models;
using System.Collections.Generic;
using System.IO;

namespace EQTool.Services
{
    public static class ParseSpells
    {
        public static List<SpellBase> GetSpells()
        {
            var ret = new List<SpellBase>();
            var spellastext = File.ReadAllLines(FindEq.BestGuessRootEqPath + "/spells_us.txt");
            foreach (var item in spellastext)
            {
                var splits = item.Split('^');
                var spell = new SpellBase
                {
                    id = int.Parse(splits[0]),
                    name = splits[1].ToLower(),
                    buffduration = int.Parse(splits[17]),
                    buffdurationformula = int.Parse(splits[16]),
                    cast_on_other = splits[7],
                    cast_on_you = splits[6],
                    spell_fades = splits[8],
                    spell_icon = int.Parse(splits[144])
                };
                ret.Add(spell);
            }

            return ret;
        }

    }
}
