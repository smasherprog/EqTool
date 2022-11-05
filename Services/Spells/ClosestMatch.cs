using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Spells
{
    public static class ClosestMatch
    {
        public static Spell GetClosestmatch(List<Spell> spells, int? playerlevel)
        {
            if (!playerlevel.HasValue)
            {
                return spells.FirstOrDefault(a => a.Level > 0 && a.Level <= 60);
            }

            Spell closestspell = null;
            var leveldelta = 100;
            var guessnumber = 0;
            foreach (var item in spells)
            {
                guessnumber++;
                var delta = Math.Abs(item.Level - playerlevel.Value);
                if (delta < leveldelta)
                {
                    leveldelta = delta;
                    closestspell = item;
                }
            }

            return closestspell;
        }

    }
}
