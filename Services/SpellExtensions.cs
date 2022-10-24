using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public static class SpellExtensions
    {
        public static Spell Map(this SpellBase spell, List<SpellIcon> spellIcons)
        {
            var spellfilenumber = (int)Math.Ceiling(spell.spell_icon / (float)36);

            var s = new Spell
            {
                name = spell.name,
                buffduration = spell.buffduration,
                buffdurationformula = spell.buffdurationformula,
                cast_on_other = spell.cast_on_other,
                cast_on_you = spell.cast_on_you,
                spell_fades = spell.spell_fades,
                spell_icon = spell.spell_icon,
                casttime = spell.casttime,
                Level = spell.Level,
                id = spell.id,
                Rect = new System.Windows.Int32Rect(),
                SpellIcon = null,
                pvp_buffdurationformula = spell.pvp_buffdurationformula,
                type = spell.type
            };

            if (spellfilenumber > 0 && spellfilenumber <= 7)
            {
                var spellnumber = spell.spell_icon % 36;
                var filerow = (int)Math.Floor((spellnumber + 6) / (float)6);
                var filecol = (spellnumber % 6) + 1;
                var x = (filecol - 1) * 40;
                var y = (filerow - 1) * 40;
                s.Rect = new System.Windows.Int32Rect(x, y, 40, 40);
                s.SpellIcon = spellIcons.FirstOrDefault(a => a.SpellIndex == spellfilenumber);
            }

            return s;
        }
    }
}
