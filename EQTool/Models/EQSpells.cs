using EQTool.Services;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Models
{
    public class EQSpells
    {
        public const string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";

        public const string Your = "Your ";
        public const string You = "You";
        public const string SpaceYou = " You ";
        public const string SpellHasWornoff = "spell has worn off.";
        private const string InvisMessage = " fades away";
        
        // If it's this long they deserve a timer.
        public static TimeSpan MinimumRecastForYouCooldownTimer = TimeSpan.FromSeconds(18);
        public static TimeSpan MinimumRecastForOtherCooldownTimer = TimeSpan.FromSeconds(60);
        
        // spells that we wish to count how many times they have been cast
        public static readonly List<string> SpellsThatNeedCounts = new List<string>
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
            "Rage of Vallon",
            "Waves of the Deep Sea",
            "Anarchy",
            "Breath of the Sea",
            "Frostbite",
            "Judgment of Ice",
            "Storm Strike",
            "Shrieking Howl",
            "Static Strike",
            "Rage of Zek",
            "Blinding Luminance",
            "Flash of Light"
        };

        // all the charm spells
        public static readonly List<string> Charms = new List<string>
        {
            "Dictate",
            "Charm",
            "Beguile",
            "Cajoling Whispers",
            "Allure",
            "Boltran`s Agacerie",
            "Befriend Animal",
            "Charm Animals",
            "Beguile Plants",
            "Beguile Animals",
            "Allure of the Wild",
            "Call of Karana",
            "Tunare's Request",
            "Dominate Undead",
            "Beguile Undead",
            "Cajole Undead",
            "Thrall of Bones",
            "Enslave Death"
        };

        // all the paci spells, which we treat like detrimental even though they aren't.
        public static readonly List<string> Lulls = new List<string>
        {
            "Lull Animal",
            "Calm Animal",
            "Harmony",
            "Numb the Dead",
            "Rest the Dead",
            "Lull",
            "Soothe",
            "Calm",
            "Pacify",
            "Wake of Tranquility"
        };
        
        public static readonly List<string> IllusionPartialNames = new List<string>
        {
            "Illusion",
            "Boon of the Garou",
            "Form of the",
            "Wolf Form",
            "Call of Bones",
            "Lich"
        };
        
        public static readonly List<string> SpellsThatDontEmitCompletionMessages = new List<string>(
            Charms.Concat(new [] {
                "Harmshield",
                "Divine Aura",
                "Harmony"
            }));
        
        private readonly Dictionary<string, Spell> _AllSpells = new Dictionary<string, Spell>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Spell> AllSpells
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
        
        private readonly Dictionary<string, List<Spell>> _CastOtherSpells = new Dictionary<string, List<Spell>>(StringComparer.OrdinalIgnoreCase);
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
        
        private readonly Dictionary<string, List<Spell>> _CastOnYouSpells = new Dictionary<string, List<Spell>>(StringComparer.OrdinalIgnoreCase);
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

        private readonly Dictionary<string, List<Spell>> _YouCastSpells = new Dictionary<string, List<Spell>>(StringComparer.OrdinalIgnoreCase);
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

        private readonly Dictionary<string, List<Spell>> _WornOffSpells = new Dictionary<string, List<Spell>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<Spell>> WornOffSpells
        {
            get
            {
                if (!_WornOffSpells.Any())
                {
                    BuildSpellInfo();
                }
                return _WornOffSpells;
            }
        }
        
        private readonly ParseSpells_spells_us parseSpells;
        private readonly SpellIcons spellIcons;

        public EQSpells(ParseSpells_spells_us parseSpells, SpellIcons spellIcons)
        {
            this.parseSpells = parseSpells;
            this.spellIcons = spellIcons;
        }

        private void BuildSpellInfo()
        {
            BuildSpellInfo(Servers.Green);
        }

        public void BuildSpellInfo(Servers servers)
        {
            var spellicons = spellIcons.GetSpellIcons(servers);
            var spells = parseSpells.GetSpells(servers);
            foreach (var item in spells)
            {
                var mappedspell = item.Map(spellicons);
                if (!mappedspell.HasSpellIcon)
                {
                    continue;
                }

                // The duplicate key check should only be relevant for "no spell" entries (which we are already handling and never look up directly anyways), but just in case we'll handle them all the same.
                var key = mappedspell.name;
                if (mappedspell.name.Equals("no spell", StringComparison.OrdinalIgnoreCase) || _AllSpells.ContainsKey(mappedspell.name))
                {
                    key = $"{mappedspell.name}#{mappedspell.id}";
                }
                _AllSpells.Add(key, mappedspell);

                if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_other))
                {
                    if (mappedspell.cast_on_other.Contains(InvisMessage))
                    {
                        Debug.WriteLine("Skipping Other invis spell. Cant detect difference between gate and invis");
                    }
                    else if (_CastOtherSpells.TryGetValue(mappedspell.cast_on_other, out _))
                    {
                        _CastOtherSpells[mappedspell.cast_on_other].Add(mappedspell);
                    }
                    else
                    {
                        _CastOtherSpells.Add(mappedspell.cast_on_other, new List<Spell> { mappedspell });
                    }
                }
                if (!string.IsNullOrWhiteSpace(mappedspell.name) && mappedspell.Classes.Any(a => a.Value > 0))
                {
                    if (_YouCastSpells.TryGetValue(mappedspell.name, out _))
                    {
                        _YouCastSpells[mappedspell.name].Add(mappedspell);
                    }
                    else
                    {
                        _YouCastSpells.Add(mappedspell.name, new List<Spell> { mappedspell });
                    }
                }
                if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_you))
                {
                    if (_CastOnYouSpells.TryGetValue(mappedspell.cast_on_you, out _))
                    {
                        if (!(mappedspell.Classes.Any() && mappedspell.SpellType == SpellType.Self))
                        {
                            _CastOnYouSpells[mappedspell.cast_on_you].Add(mappedspell);
                        }
                    }
                    else
                    {
                        _CastOnYouSpells.Add(mappedspell.cast_on_you, new List<Spell> { mappedspell });
                    }
                }
                if (!string.IsNullOrWhiteSpace(mappedspell.spell_fades))
                {
                    if (_WornOffSpells.TryGetValue(mappedspell.spell_fades, out _))
                    {
                        _WornOffSpells[mappedspell.spell_fades].Add(mappedspell);
                    }
                    else
                    {
                        _WornOffSpells.Add(mappedspell.spell_fades, new List<Spell> { mappedspell });
                    }
                }
            }
        }
    }
}
