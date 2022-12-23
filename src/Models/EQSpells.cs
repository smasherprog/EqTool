using EQTool.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Models
{
    public class EQSpells
    {
        private readonly List<Spell> _AllSpells = new List<Spell>();

        public List<Spell> AllSpells
        {
            get
            {
                if (!_AllSpells.Any())
                {
                    BuildSpellInfo();
                }
                return _AllSpells;
            }
        }
        private readonly Dictionary<string, List<Spell>> _CastOtherSpells = new Dictionary<string, List<Spell>>();

        public Dictionary<string, List<Spell>> CastOtherSpells
        {
            get
            {
                if (!_CastOtherSpells.Any())
                {
                    BuildSpellInfo();
                }
                return _CastOtherSpells;
            }
        }
        private readonly Dictionary<string, List<Spell>> _CastOnYouSpells = new Dictionary<string, List<Spell>>();

        public Dictionary<string, List<Spell>> CastOnYouSpells
        {
            get
            {
                if (!_CastOnYouSpells.Any())
                {
                    BuildSpellInfo();
                }
                return _CastOnYouSpells;
            }
        }

        private readonly Dictionary<string, List<Spell>> _YouCastSpells = new Dictionary<string, List<Spell>>();
        public Dictionary<string, List<Spell>> YouCastSpells
        {
            get
            {
                if (!_YouCastSpells.Any())
                {
                    BuildSpellInfo();
                }
                return _YouCastSpells;
            }
        }

        private readonly List<string> IgnoreSpellsList = new List<string>()
        {
            "Complete Heal",
            "Denon`s Disruptive Discord",
            "Chords of Dissonance"
        };

        public static readonly Dictionary<string, PlayerClasses> EpicSpells = new Dictionary<string, PlayerClasses>
        {
            { "Wrath of Nature", PlayerClasses.Druid },
            { "Speed of the Shissar", PlayerClasses.Enchanter },
            { "Torment of Shadows", PlayerClasses.Necromancer },
            { "Earthcall", PlayerClasses.Ranger },
            { "Soul Consumption", PlayerClasses.ShadowKnight },
            { "Curse of the Spirits", PlayerClasses.Shaman },
            { "Barrier of Force", PlayerClasses.Wizard },
            { "Dance of the Blade", PlayerClasses.Bard },
            { "Celestial Tranquility", PlayerClasses.Monk },
            { "Seething Fury", PlayerClasses.Rogue }
        };

        public const string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";
        public const string YouBeginCasting = "You begin casting ";
        public const string Your = "Your ";
        public const string You = "You ";
        public const string SpaceYou = " You ";
        private const string InvisMessage = " fades away";
        private readonly ParseSpells_spells_us parseSpells;
        private readonly SpellIcons spellIcons;

        public EQSpells(ParseSpells_spells_us parseSpells, SpellIcons spellIcons)
        {
            this.parseSpells = parseSpells;
            this.spellIcons = spellIcons;
            BuildSpellInfo();
        }

        private void BuildSpellInfo()
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
                _AllSpells.Add(mappedspell);
                if (mappedspell.buffduration > 0)
                {
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_other))
                    {
                        if (mappedspell.cast_on_other.Contains(InvisMessage))
                        {
                            Debug.WriteLine("Skipping Other invis spell. Cant detect difference between gate and invis");
                        }
                        else if (_CastOtherSpells.TryGetValue(mappedspell.cast_on_other, out var innerval))
                        {
                            _CastOtherSpells[mappedspell.cast_on_other].Add(mappedspell);
                        }
                        else
                        {
                            _CastOtherSpells.Add(mappedspell.cast_on_other, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.name) && mappedspell.Classes.Any(a => a.Value > 0))
                    {
                        if (_YouCastSpells.TryGetValue(mappedspell.name, out var innerval))
                        {
                            _YouCastSpells[mappedspell.name].Add(mappedspell);
                        }
                        else
                        {
                            _YouCastSpells.Add(mappedspell.name, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_you))
                    {
                        if (_CastOnYouSpells.TryGetValue(mappedspell.cast_on_you, out var innerval))
                        {
                            _CastOnYouSpells[mappedspell.cast_on_you].Add(mappedspell);
                        }
                        else
                        {
                            _CastOnYouSpells.Add(mappedspell.cast_on_you, new List<Spell>() { mappedspell });
                        }
                    }
                }
                else
                {
                   // Debug.WriteLine($"Spell {mappedspell.name} Ignored");
                }
            }
        }
    }
}
