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

        public const string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";

        public const string Your = "Your ";
        public const string You = "You";
        public const string SpaceYou = " You ";
        public const string SpellHasWornoff = "spell has worn off.";
        private const string InvisMessage = " fades away";
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
                _AllSpells.Add(mappedspell);

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
                        if (!(mappedspell.Classes.Any() && mappedspell.SpellType == SpellType.Self))
                        {
                            _CastOnYouSpells[mappedspell.cast_on_you].Add(mappedspell);
                        }
                    }
                    else
                    {
                        _CastOnYouSpells.Add(mappedspell.cast_on_you, new List<Spell>() { mappedspell });
                    }
                }
                if (!string.IsNullOrWhiteSpace(mappedspell.spell_fades))
                {
                    if (_WornOffSpells.TryGetValue(mappedspell.spell_fades, out var innerval))
                    {
                        _WornOffSpells[mappedspell.spell_fades].Add(mappedspell);
                    }
                    else
                    {
                        _WornOffSpells.Add(mappedspell.spell_fades, new List<Spell>() { mappedspell });
                    }
                }
            }
        }
    }
}
