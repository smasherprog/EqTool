using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EQTool.Services
{
    public class ParseSpells_spells_us
    {
        private List<SpellBase> _Spells = new List<SpellBase>();
        private readonly EQToolSettings settings;

        public ParseSpells_spells_us(EQToolSettings settings)
        {
            this.settings = settings;
        }

        private readonly HashSet<string> IgnoreSpells = new HashSet<string>()
        {
            "FireElementalAttack2",
            "Outbreak",
            "Paroxysm of Zek",
            "Dark Madness",
            "Peace of the Disciple Strike",
            "HandOfHolyVengeanceIRecourse",
            "HandOfHolyVengeanceIIRecourse",
            "HandOfHolyVengeanceIVRecourse",
            "HandOfHolyVengeanceVRecourse",
            "HandOfHolyVengeanceIRecourse",
            "Complete Heal",
            "Denon`s Disruptive Discord",
            "Chords of Dissonance",
            "Infection Test 1",
            "Infection Test 2",
            "Levitate Test",
            "Test GLT",
            "Test GMD",
            "Test Shield",
            "Test GACD",
            "Bond of Sathir",
            "Soul Consumption R.",
            "Soul Claw Strike",
            "Malevolent Vex"
        };

        private readonly List<string> IgnoreRomanNumerals = new List<string>()
        {
            " I",
            " II",
            " III",
            " IV",
           " V",
           " VI",
           " VII",
           " VIII",
           " IX",
           " X",
            " X",
            " XI",
            " XII",
            " XIII",
            " XIV"
        };
        private readonly List<string> GoodRomanNumeralSpells = new List<string>()
        {
            "Cannibalize",
            "Rune",
            "Yaulp",
            "Burnout",
            "Contact Poison",
            "Berserker Madness",
            "Brittle Haste",
            "Feeble Mind",
            "Injected Poison",
            "Clarity",
            "Monster Summoning",
            "Dizzy",
            "Berserker Madness",
            "Blinding Poison",
            "Feeble Mind"
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
        private readonly List<SpellType> IgnoreSpellTypes = new List<SpellType>()
        {
              SpellType.RagZhezumSpecial
        };


        public List<SpellBase> GetSpells()
        {
            if (_Spells.Any())
            {
                return _Spells;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var spells = new Dictionary<string, SpellBase>();
            var spellsfile = new FileInfo(settings.DefaultEqDirectory + "/spells_us.txt");
            if (spellsfile.Exists)
            {
                var spellastext = File.ReadAllLines(settings.DefaultEqDirectory + "/spells_us.txt");
                var desctypes = new List<DescrNumber>() {
                 DescrNumber.ThePlanes,
                 DescrNumber.Luclin,
                 DescrNumber.Taelosia,
                 DescrNumber.Discord
                };
                foreach (var item in spellastext)
                {
                    var spell = ParseLine(item);
                    if (string.IsNullOrWhiteSpace(spell.name) &&
                        string.IsNullOrWhiteSpace(spell.cast_on_you) &&
                        spell.buffduration <= 0 &&
                        spell.spell_icon <= 0
                        )
                    {
                        continue;
                    }
                    if (spell.Classes.Any() && spell.Classes.All(a => a.Value > 60 && a.Value <= 255))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} Class Level Out of range");
                        continue;
                    }

                    if (spell.name.StartsWith("GM "))
                    {
                        // Debug.WriteLine($"Skipping {spell.name} GM");
                        continue;
                    }

                    if (spell.name.StartsWith("Guide "))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} Guide");
                        continue;
                    }

                    if (spell.name.StartsWith("NPC"))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} NPC");
                        continue;
                    }

                    if (spell.resisttype > ResistType.DISEASE)
                    {
                        //Debug.WriteLine($"Skipping {spell.name} ResistType");
                        continue;
                    }

                    if (!spell.name.StartsWith("Alter Plane") && (desctypes.Contains(spell.DescrNumber) || spell.DescrNumber >= DescrNumber.Traps2))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} DescrNumber");
                        continue;
                    }

                    if (IgnoreSpells.Contains(spell.name))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} DescrNumber");
                        continue;
                    }

                    if (!GoodRomanNumeralSpells.Any(a => spell.name.StartsWith(a)) && IgnoreRomanNumerals.Any(a => spell.name.EndsWith(a)))
                    {
                        //Debug.WriteLine($"Skipping {spell.name} RomanNumerals");
                        continue;
                    }

                    if (IgnoreSpellTypes.Contains(spell.SpellType))
                    {
                        //Debug.WriteLine($"Skipping {spell.SpellType} {spell.name} IgnoreSpellTypes");
                        continue;
                    }

                    if (spells.TryGetValue(spell.name, out var spellinlist))
                    {
                        if (spellinlist.Classes.Any() && !spell.Classes.Any())
                        {
                            //Debug.WriteLine($"NOT updating Duplicate Spell {spell.name}");
                        }
                        else
                        {
                            //Debug.WriteLine($"Duplicate Spell Update {spell.name}");
                            //skippedcounter++;
                            spells[spell.name] = spell;
                        }
                    }
                    else
                    {
                        spells.Add(spell.name, spell);
                    }
                }
                // Debug.WriteLine($"Skipped {skippedcounter}");

                _Spells = spells.Values.ToList();
            }
            stopwatch.Stop();
            Debug.Write($"Took {stopwatch.ElapsedMilliseconds}ms to build spells");
            return _Spells;
        }

        public static SpellBase ParseLine(string line)
        {
            var splits = line.Split('^');
            var classes = new Dictionary<PlayerClasses, int>();
            for (var i = 104; i < 104 + (int)PlayerClasses.Enchanter + 1; i++)
            {
                if (int.TryParse(splits[i], out var l))
                {
                    if (l >= 0 && l < 255)
                    {
                        var clas = (PlayerClasses)(i - 104);
                        classes.Add(clas, l);
                    }
                }
            }
            var resisttype = (ResistType)Enum.Parse(typeof(ResistType), splits[85]);
            var descrtype = (DescrNumber)Enum.Parse(typeof(DescrNumber), splits[157]);
            var spelltype = (SpellType)Enum.Parse(typeof(SpellType), splits[98]);
            var ret = new SpellBase
            {
                id = int.Parse(splits[0]),
                name = splits[1],
                buffduration = int.Parse(splits[17]),
                buffdurationformula = int.Parse(splits[16]),
                pvp_buffdurationformula = int.Parse(splits[181]),
                type = int.Parse(splits[83]),
                cast_on_other = splits[7].Trim(),
                casttime = int.Parse(splits[13]),
                cast_on_you = splits[6].Trim(),
                spell_fades = splits[8].Trim(),
                Classes = classes,
                spell_icon = int.Parse(splits[144]),
                resisttype = resisttype,
                ResistCheck = int.Parse(splits[147]),
                DescrNumber = descrtype,
                SpellType = spelltype
            };

            if (EpicSpells.TryGetValue(ret.name, out var foundepic))
            {
                ret.Classes[foundepic] = 46;
            }

            return ret;
        }
    }
}
