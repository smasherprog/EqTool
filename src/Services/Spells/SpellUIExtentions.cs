using EQToolShared.Enums;
using System.Collections.Generic;

namespace EQTool.Services.Spells
{
    public static class SpellUIExtensions
    {
        public static bool HideSpell(List<PlayerClasses> showSpellsForClasses, Dictionary<PlayerClasses, int> spellclasses)
        {
            if (showSpellsForClasses == null || spellclasses == null)
            {
                return false;
            }

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
