using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public static class SpellDurations
    {
        public static Spell MatchClosestLevelToSpell(List<Spell> spells, PlayerInfo player)
        {
            var userlevel = player?.Level;
            var playerclass = player?.PlayerClass;
            if (playerclass.HasValue && userlevel.HasValue)
            {
                var closestlevel = userlevel.Value;
                Spell closestspell = null;
                foreach (var spell in spells)
                {
                    if (spell.Classes.TryGetValue(playerclass.Value, out var level))
                    {
                        return spell;
                    }

                    foreach (var item in spell.Classes)
                    {
                        var delta = Math.Abs(item.Value - closestlevel);
                        if (delta < closestlevel)
                        {
                            closestspell = spell;
                            closestlevel = delta;
                        }
                    }
                }
                return closestspell;
            }

            foreach (var item in spells)
            {
                foreach (var level in item.Classes)
                {
                    if (level.Value > 0 && level.Value <= 60)
                    {
                        return item;
                    }
                }
            }

            return spells.FirstOrDefault();
        }

        public static int MatchClosestLevelToSpell(Spell spell, PlayerInfo player)
        {
            var userlevel = player?.Level;
            var playerclass = player?.PlayerClass;
            if (playerclass.HasValue && userlevel.HasValue)
            {
                if (spell.Classes.ContainsKey(playerclass.Value))
                {
                    return userlevel.Value;
                }

                var closestlevel = userlevel.Value;
                foreach (var item in spell.Classes)
                {
                    var delta = Math.Abs(item.Value - closestlevel);
                    if (delta < closestlevel)
                    {
                        closestlevel = delta;
                    }
                }
                return closestlevel;
            }

            return spell.Classes.Values.OrderBy(a => a).FirstOrDefault();
        }

        public static int GetDuration_inSeconds(Spell spell, PlayerInfo player)
        {
            var duration = spell.buffduration;
            int spell_ticks;
            var level = MatchClosestLevelToSpell(spell, player);

            switch (spell.buffdurationformula)
            {
                case 0:
                    spell_ticks = 0;
                    break;
                case 1:
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 2:
                    spell_ticks = (int)Math.Ceiling(level / 5.0f * 3);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 3:
                    spell_ticks = level * 30;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 4:
                    spell_ticks = duration == 0 ? 50 : duration;
                    break;
                case 5:
                    spell_ticks = duration;
                    if (spell_ticks == 0)
                    {
                        spell_ticks = 3;
                    }

                    break;
                case 6:
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 7:
                    spell_ticks = level;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 8:
                    spell_ticks = level + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 9:
                    spell_ticks = (level * 2) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 10:
                    spell_ticks = (level * 3) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 11:
                case 12:
                case 15:
                    spell_ticks = duration;
                    break;
                case 50:
                    spell_ticks = 72000;
                    break;
                case 3600:
                    spell_ticks = duration == 0 ? 3600 : duration;
                    break;
                default:
                    spell_ticks = duration;
                    break;
            }

            return spell_ticks * 6;
        }
    }
}
