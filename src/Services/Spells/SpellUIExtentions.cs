using EQTool.Models;
using System.Collections.Generic;

namespace EQTool.Services.Spells
{
    public static class SpellUIExtentions
    {
        public static bool HideSpell(List<PlayerClasses> showSpellsForClasses, Dictionary<PlayerClasses, int> spellclasses)
        {
            foreach (var showspellclass in showSpellsForClasses)
            {
                if (spellclasses.ContainsKey(showspellclass))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
