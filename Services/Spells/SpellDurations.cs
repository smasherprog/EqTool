using EQTool.Models;
using System;

namespace EQTool.Services
{
    public static class SpellDurations
    {
        public static int GetDuration_inSeconds(Spell spell, int? userlevel)
        {
            _ = spell.buffdurationformula;
            var duration = spell.buffduration;
            int spell_ticks;
            var level = userlevel.HasValue ? (spell.Level > userlevel.Value ? spell.Level : userlevel.Value) : spell.Level;
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
