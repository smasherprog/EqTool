using EQTool.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Models
{
    public class EQSpells
    {
        public readonly List<Spell> AllSpells = new List<Spell>();
        public readonly Dictionary<string, List<Spell>> CastOtherSpells = new Dictionary<string, List<Spell>>();
        public readonly Dictionary<string, List<Spell>> CastOnYouSpells = new Dictionary<string, List<Spell>>();
        public readonly Dictionary<string, List<Spell>> YouCastSpells = new Dictionary<string, List<Spell>>();

        private readonly List<string> IgnoreSpellsList = new List<string>()
        {
            "Complete Heal",
            "Denon`s Disruptive Discord",
            "Chords of Dissonance"
        };

        public const string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";
        public const string YouBeginCasting = "You begin casting ";
        public const string Your = "Your ";
        public const string You = "You ";

        public EQSpells(ParseSpells_spells_us parseSpells, SpellIcons spellIcons)
        {
            var spellicons = spellIcons.GetSpellIcons();
            var spells = parseSpells.GetSpells().Where(a => !IgnoreSpellsList.Contains(a.name) && a.spell_icon > 0);
            foreach (var item in spells)
            {
                var mappedspell = item.Map(spellicons);
                if (!mappedspell.HasSpellIcon)
                {
                    continue;
                }
                AllSpells.Add(mappedspell);
                if (mappedspell.buffduration > 0)
                {
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_other))
                    {
                        if (CastOtherSpells.TryGetValue(mappedspell.cast_on_other, out var innerval))
                        {
                            CastOtherSpells[mappedspell.cast_on_other].Add(mappedspell);
                        }
                        else
                        {
                            CastOtherSpells.Add(mappedspell.cast_on_other, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.name) && mappedspell.Level > 0)
                    {
                        if (YouCastSpells.TryGetValue(mappedspell.name, out var innerval))
                        {
                            YouCastSpells[mappedspell.name].Add(mappedspell);
                        }
                        else
                        {
                            YouCastSpells.Add(mappedspell.name, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_you))
                    {
                        if (CastOnYouSpells.TryGetValue(mappedspell.cast_on_you, out var innerval))
                        {
                            CastOnYouSpells[mappedspell.cast_on_you].Add(mappedspell);
                        }
                        else
                        {
                            CastOnYouSpells.Add(mappedspell.cast_on_you, new List<Spell>() { mappedspell });
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Spell {mappedspell.name} Ignored");
                }
            }
        }
    }
}
