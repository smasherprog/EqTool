using EqTool.Models;

namespace EqTool.Services
{
    public static class ParseSpells
    {
        public static Spells GetSpells()
        {
            var ret = new Spells();
            var spellastext = File.ReadAllLines(FindEq.BestGuessRootEqPath + "/spells_us.txt");
            foreach (var item in spellastext)
            {
                var splits = item.Split("^");
                var spell = new Spell
                {
                    id = int.Parse(splits[0]),
                    name = splits[1].ToLower(),
                    buffduration = int.Parse(splits[17]),
                    buffdurationformula = int.Parse(splits[16]),
                    cast_on_other = splits[7],
                    cast_on_you = splits[6],
                    spell_fades = splits[8]
                };
                ret.SpellList.Add(spell);
            }

            return ret;
        }

    }
}
