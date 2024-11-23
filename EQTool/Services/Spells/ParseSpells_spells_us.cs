using EQTool.Models;
using EQTool.ViewModels;
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
        private readonly LoggingService loggingService;
        private readonly ActivePlayer activePlayer;

        public ParseSpells_spells_us(EQToolSettings settings, LoggingService loggingService, ActivePlayer activePlayer)
        {
            this.settings = settings;
            this.loggingService = loggingService;
            this.activePlayer = activePlayer;
        }

        private readonly HashSet<string> IgnoreSpells = new HashSet<string>()
        {
            "Shield of the Ring",
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
            "Malevolent Vex",
            "Caustic Mist",
            "Ojun Roar",
            "Dragon Bellow",
            "Ice Comet",
            "IceBoneFrostBurst",
            "FrostAOE",
            "Ring of Winter",
            "Frost Shards",
            "Umbral Rot",
            "Grimling Rot",
            "Crystal Roar",
            "Rimebone Frost Burst",
            "Trushar's Frost",
            "Icicle Shock",
            "Shock of Frost",
            "Talendor's Immolating Breath",
            "Lava Breath - Test",
            "Vengeance of the Undying",
            "Gift of A'err",
        };
        private readonly List<int> IgnoreIds = new List<int>()
        {
            6615
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

        private readonly List<string> POMFlowers = new List<string>()
        {
            "Aura of Blue Petals",
            "Aura of White Petals",
            "Aura of Red Petals",
            "Aura of Black Petals"
        };

        public class EpicSpellTime
        {
            public PlayerClasses PlayerClass { get; set; }
            public int CastTime { get; set; }
        }

        public static readonly Dictionary<string, EpicSpellTime> EpicSpells = new Dictionary<string, EpicSpellTime>
        {
            { "Wrath of Nature", new EpicSpellTime{ CastTime = 9000, PlayerClass= PlayerClasses.Druid } },
            { "Speed of the Shissar", new EpicSpellTime{ CastTime = 6000, PlayerClass= PlayerClasses.Enchanter } },
            { "Torment of Shadows",new EpicSpellTime{ CastTime = 9000, PlayerClass=  PlayerClasses.Necromancer } },
            { "Earthcall", new EpicSpellTime{ CastTime = 0, PlayerClass= PlayerClasses.Ranger } },
            { "Soul Consumption",new EpicSpellTime{ CastTime = 0, PlayerClass=  PlayerClasses.ShadowKnight } },
            { "Curse of the Spirits", new EpicSpellTime{ CastTime = 9000, PlayerClass= PlayerClasses.Shaman } },
            { "Barrier of Force", new EpicSpellTime{ CastTime = 15000, PlayerClass= PlayerClasses.Wizard } },
            { "Dance of the Blade", new EpicSpellTime{ CastTime = 0, PlayerClass= PlayerClasses.Bard } },
            { "Celestial Tranquility", new EpicSpellTime{ CastTime = 0, PlayerClass= PlayerClasses.Monk } },
            { "Seething Fury", new EpicSpellTime{ CastTime = 0, PlayerClass= PlayerClasses.Rogue } }
        };
        private readonly List<SpellType> IgnoreSpellTypes = new List<SpellType>()
        {
              SpellType.RagZhezumSpecial
        };


        public List<SpellBase> GetSpells(Servers servers)
        {
            if (_Spells.Any())
            {
                return _Spells;
            }
            var isdebug = false;
#if DEBUG
            isdebug = true;
#endif

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var spells = new Dictionary<string, SpellBase>();
            var spellfile = "/spells_us.txt";
            var spellsfile = new FileInfo(settings.DefaultEqDirectory + spellfile);
            if (spellsfile.Exists)
            {
                var spellfilename = $"SpellCache{servers}_9";
                if (!isdebug)
                {
                    spellfilename = new string(spellfilename.Where(a => char.IsLetterOrDigit(a)).ToArray()) + ".bin";
                    if (File.Exists(spellfilename))
                    {
                        try
                        {
                            _Spells = BinarySerializer.ReadFromBinaryFile<List<SpellBase>>(spellfilename);
                            stopwatch.Stop();
                            Debug.Write($"Took {stopwatch.ElapsedMilliseconds}ms to build spells");
                            return _Spells;
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
                        }
                    }
                }

                var spellastext = File.ReadAllLines(settings.DefaultEqDirectory + spellfile);
                var desctypes = new List<DescrNumber>() {
                 DescrNumber.ThePlanes,
                 DescrNumber.Luclin,
                 DescrNumber.Taelosia,
                 DescrNumber.Discord
                };
                foreach (var item in spellastext)
                {
                    var spell = ParseP99Line(item);
                    if (spell == null || (string.IsNullOrWhiteSpace(spell.name) &&
                        string.IsNullOrWhiteSpace(spell.cast_on_you) &&
                        spell.buffduration <= 0 &&
                        spell.spell_icon <= 0
                        ))
                    {
                        continue;
                    }

                    if (IgnoreIds.Contains(spell.id))
                    {
                        continue;
                    }

                    if (spell.name == "Defensive Discipline")
                    {
                        if (spell.cast_on_you.EndsWith(".."))
                        {
                            spell.cast_on_you = spell.cast_on_you.Replace("..", ".");
                        }
                    }

                    if (spell.name.StartsWith("Primal Essence"))
                    {
                        if (!spell.Classes.ContainsKey(PlayerClasses.Shaman))
                        {
                            spell.Classes.Add(PlayerClasses.Shaman, 35);
                        }
                    }

                    if (spell.name == "LowerElement")
                    {
                        if (!spell.Classes.ContainsKey(PlayerClasses.Wizard))
                        {
                            spell.Classes.Add(PlayerClasses.Wizard, 51);
                        }
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

                    if (spell.name.IndexOf("test", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        //Debug.WriteLine($"Skipping {spell.name} NPC");
                        continue;
                    }

                    if (spell.resisttype > ResistType.DISEASE)
                    {
                        //Debug.WriteLine($"Skipping {spell.name} ResistType");
                        continue;
                    }

                    if (!spell.name.StartsWith("Alter Plane") && !spell.name.StartsWith("Primal Essence") && (desctypes.Contains(spell.DescrNumber) || spell.DescrNumber >= DescrNumber.Traps2))
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

                    if (spell.name == "Levitate")
                    {
                        var peggylev = ParseP99Line(item);
                        peggylev.name = "Peggy Levitate";
                        peggylev.buffduration = 120;
                        peggylev.buffdurationformula = 12;
                        peggylev.casttime = 6000;
                        if (!spells.ContainsKey(peggylev.name))
                        {
                            spells.Add(peggylev.name, peggylev);
                        }
                    }

                    if (POMFlowers.Contains(spell.name))
                    {
                        if (!spell.Classes.ContainsKey(PlayerClasses.Other))
                        {
                            spell.Classes.Add(PlayerClasses.Other, 46);
                        }
                    }

                    if (spell.name == "Maniacal Strength")
                    {
                        spell.name = "Manicial Strength";
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
                stopwatch.Stop();
                Debug.Write($"Took {stopwatch.ElapsedMilliseconds}ms to build spells");
                try
                {
                    var filetodelete = Directory.GetFiles(Directory.GetCurrentDirectory(), "SpellCache*", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    File.Delete(filetodelete);
                }
                catch (Exception)
                {

                }
                try
                {
                    BinarySerializer.WriteToBinaryFile(spellfilename, _Spells);
                }
                catch (Exception ex)
                {
                    loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
                }
            }

            return _Spells;
        }

        private static SpellBase ParseLine(string line, int offset, int spelliconoffset)
        {
            try
            {
                var splits = line.Split('^');
                var classes = new Dictionary<PlayerClasses, int>();
                for (var i = 104 - offset; i < 104 - offset + (int)PlayerClasses.Enchanter + 1; i++)
                {
                    if (int.TryParse(splits[i], out var l))
                    {
                        if (l >= 0 && l < 255)
                        {
                            var clas = (PlayerClasses)(i - 104 + offset);
                            classes.Add(clas, l);
                        }
                    }
                }
                var resisttype = (ResistType)Enum.Parse(typeof(ResistType), splits[85 - offset]);
                var descrtype = splits.Length >= 157 ? (DescrNumber)Enum.Parse(typeof(DescrNumber), splits[157 - offset]) : 0;
                var spelltype = (SpellType)Enum.Parse(typeof(SpellType), splits[98 - offset]);


                var id = int.Parse(splits[0]);
                var buffduration = int.Parse(splits[17]);
                var buffdurationformula = int.Parse(splits[16]);
                var pvp_buffdurationformula = (splits.Length >= 181) ? int.Parse(splits[181 - offset]) : 0;
                var type = int.Parse(splits[83 - offset]);
                var casttime = int.Parse(splits[13]);
                var cast_on_other = splits[7].Trim();
                var cast_on_you = splits[6].Trim();
                var spell_fades = splits[8].Trim();
                var spell_icon = int.Parse(splits[144 - spelliconoffset]);
                var ResistCheck = int.Parse(splits[147 - offset]);
                var recasttime = int.Parse(splits[15]);
                var ret = new SpellBase
                {
                    id = id,
                    name = splits[1],
                    buffduration = buffduration,
                    buffdurationformula = buffdurationformula,
                    pvp_buffdurationformula = pvp_buffdurationformula,
                    type = (SpellTypes)type,
                    cast_on_other = cast_on_other,
                    casttime = casttime,
                    cast_on_you = cast_on_you,
                    spell_fades = spell_fades,
                    Classes = classes,
                    spell_icon = spell_icon,
                    resisttype = resisttype,
                    ResistCheck = ResistCheck,
                    DescrNumber = descrtype,
                    SpellType = spelltype,
                    recastTime = recasttime
                };

                if (EpicSpells.TryGetValue(ret.name, out var foundepic))
                {
                    ret.casttime = foundepic.CastTime;
                    ret.Classes[foundepic.PlayerClass] = 46;
                }

                return ret;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }

        public static SpellBase ParseP99Line(string line)
        {
            return ParseLine(line, 0, 0);
        }
    }
}
