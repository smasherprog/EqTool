using EqTool.Models;

namespace EqTool.Services
{
    public static class ParseSpells
    {
        public static Spells GetSpells()
        {
            _ = File.ReadAllText(FindEq.BestGuessRootEqPath + "/spells_us.txt");

            return new Spells
            {
                SpellList = new List<Spell>()
            };
        }

    }
}
