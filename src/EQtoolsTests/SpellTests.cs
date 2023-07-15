using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using EQToolShared.Enums;
using EQToolShared.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQToolTests
{
    [TestClass]
    public class SpellTests
    {
        private readonly IContainer container;
        public SpellTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var spells = container.Resolve<EQSpells>();
            Assert.IsNotNull(spells);
            Assert.IsNotNull(spells.AllSpells);
            Assert.IsTrue(spells.AllSpells.Any());
        }

        [TestMethod]
        public void ExtractSpellNamesToFile()
        {
            var spells = container.Resolve<EQSpells>();
            var strspell = string.Join(",", spells.AllSpells.Select(a => a.name).ToList());
            //System.IO.File.WriteAllText("SpellNames.txt", strspell);
        }

        [TestMethod]
        public void ExtractZonesToFile()
        {
            _ = container.Resolve<WikiApi>();
            var allkeys = ZoneParser.ZoneInfoMap.Keys
                .Concat(ZoneParser.ZoneNameMapper.Keys)
                 .Concat(ZoneParser.ZoneWhoMapper.Keys)
                 .Distinct()
                 .ToList();
            foreach (var item in allkeys)
            {
                //var apireturn = api.GetData(item);
                //if (!string.IsNullOrWhiteSpace(apireturn))
                //{
                //    System.IO.File.WriteAllText($"c:/test/{item}.txt", apireturn);
                //}
            }
        }

        [TestMethod]
        public void ExtractZonesToFile1()
        {
            //var api = container.Resolve<WikiApi>();
            //var allkeys = ZoneParser.NoZoneZHelping.Keys
            // .Concat(ZoneParser.ZoneNameMapper.Keys)
            //  .Concat(ZoneParser.ZoneWhoMapper.Keys)
            //  .Distinct()
            //  .ToList();
            //var list = new List<ZoneInfo>();
            //var notablenpc = "! ''' Notable NPCs: '''";
            //foreach (var item in allkeys)
            //{
            //    var zoneinfo = new ZoneInfo
            //    {
            //        Name = item
            //    };
            //    if (!System.IO.File.Exists($"c:/test/{item}.txt"))
            //    {
            //        continue;
            //    }
            //    var apireturn = System.IO.File.ReadAllText($"c:/test/{item}.txt");
            //    apireturn = apireturn.Replace("\r", "").Replace("\n", "");
            //    var notablenpcindex = apireturn.IndexOf(notablenpc);
            //    if (notablenpcindex != -1)
            //    {
            //        var rest = apireturn.Substring(notablenpcindex + notablenpc.Length);
            //        notablenpcindex = rest.IndexOf("|");
            //        if (notablenpcindex != -1)
            //        {
            //            rest = rest.Substring(notablenpcindex + 1);
            //            notablenpcindex = rest.IndexOf("|");
            //            if (notablenpcindex != -1)
            //            {
            //                rest = rest.Substring(0, notablenpcindex);
            //                rest = rest.Replace("'''", "");
            //                var namessplit1 = rest.Split(',')
            //                  .Where(a => a.Contains("[[") && a.Contains("]]"))
            //                  .ToList();
            //                var namessplit = new List<string>();
            //                foreach (var iitssd in namessplit1)
            //                {
            //                    var beg = iitssd.IndexOf("[[") + 2;
            //                    var end = iitssd.IndexOf("]]");
            //                    var t = iitssd.Substring(beg, end - beg).Trim();
            //                    namessplit.Add(t);
            //                }
            //                zoneinfo.NotableNPCs = namessplit;
            //            }
            //        }
            //    }
            //    list.Add(zoneinfo);
            //}

            //foreach (var item in list)
            //{
            //    Debug.WriteLine(item.Name);
            //    Debug.WriteLine(string.Join(",", item.NotableNPCs));
            //}
        }

        [TestMethod]
        public void TestParseGrimAura()
        {
            _ = container.Resolve<EQSpells>();
            var line = "8639^Grim Aura^PLAYER_1^^^^A dull aura covers your hand.^'s hand is covered with a dull aura.^The grim aura fades.^0^0^0^0^3000^2250^2250^3^270^0^25^3^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^10^0^0^0^0^0^0^0^0^0^0^0^2503^2108^-1^-1^-1^-1^1^1^1^1^-1^-1^-1^-1^102^100^100^100^100^100^100^100^100^100^100^100^0^1^0^0^2^254^254^254^254^254^254^254^254^254^254^254^6^25^5^-1^0^0^255^255^255^255^22^255^255^255^255^255^4^255^255^255^255^255^43^0^0^8^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^100^0^37^94^0^0^0^0^0^0^0^0^0^0^0^7^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^5^101^12^92^3^270^0^0^0^0^3^105^0^0^0^0^0^0^0^0^0^0^1^1^0^0^0^0^0^-1^0^0^0^1^0^0^1^1^^0";
            var grimaura = ParseSpells_spells_us.ParseLine(line);
            Assert.IsNotNull(grimaura);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_GrimAura()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, new PlayerInfo
            {
                Level = 30,
                PlayerClass = PlayerClasses.ShadowKnight
            });
            Assert.AreEqual(ret, 30);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Journeymansboots()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "JourneymanBoots";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(spell, new PlayerInfo
            {
                Level = 35,
                PlayerClass = PlayerClasses.ShadowKnight
            });
            Assert.AreEqual(ret, 35);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Alliance()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Alliance";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other);
            var player = new PlayerInfo
            {
                Level = 35,
                PlayerClass = PlayerClasses.ShadowKnight
            };
            var duration = SpellDurations.GetDuration_inSeconds(spell, player);
            Assert.AreEqual(duration, 0);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlyManaSieve()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Mana Sieve";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other).ToList();
            Assert.AreEqual(1, spell1.Count);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk60_GrimAura()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.ShadowKnight
            });
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk60_NaltronMark()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Naltron's Mark";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other).ToList();
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(spell1, new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.ShadowKnight
            });
            Assert.AreEqual(ret.name, "Symbol of Naltron");
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro1_GrimAura_weird_but_Shouldhandle()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, new PlayerInfo
            {
                Level = 1,
                PlayerClass = PlayerClasses.Necromancer
            });
            Assert.AreEqual(ret, 4);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro60_GrimAura()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            });
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectly_Level60_GrimAura_NoClass()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, new PlayerInfo
            {
                Level = 60,
                PlayerClass = null
            });
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestWarriorDiscipline()
        {
            _ = container.Resolve<EQSpells>();
            var line = "jobob assumes an evasive fighting style.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell(line);

            Assert.IsNotNull(guess);
        }

        [TestMethod]
        public void TestManaSeive()
        {
            _ = container.Resolve<EQSpells>();
            var line = "An ancient Frost guardian staggers in pain.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Enchanter
            };
            var guess = service.HandleBestGuessSpell(line);

            Assert.IsNotNull(guess);
        }

        [TestMethod]
        public void TestClericAego()
        {
            var spells = container.Resolve<EQSpells>();
            var aego = "Aegolism";
            var aegospell = spells.AllSpells.FirstOrDefault(a => a.name == aego);
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell(aegospell.cast_on_other);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestCallOfThePredetorAego()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Call of the Predator";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestResistMagic()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Group Resist Magic";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSavageSpirit()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Savage Spirit";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar()
        {
            var spells = container.Resolve<EQSpells>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = service.HandleBestGuessSpell(shissarspell.cast_on_other);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = spelllogparse.MatchSpell(line);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForShadowKnight()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Shaman
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_onespace()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.AreEqual(guess.TargetName, "an Jobober");
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_twospace()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.AreEqual(guess.TargetName, "an Jobober rager");
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_threespace()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var spellname = "Cripple";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.AreEqual(7, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.AreEqual(guess.TargetName, "an Jobober rager");
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar2()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A bottomless feaster's body pulses with the spirit of the Shissar.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A Ratling is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic2()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Gkrean Prophet of Tallon is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.IsNotNull(guess);
            Assert.AreEqual("Gkrean Prophet of Tallon", guess.TargetName);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A rat Ratling is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanAcumen()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var spellname = "Acumen";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = EQSpells.YouBeginCasting + " " + spellname;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Shaman
            };
            var guess = spelllogparse.MatchSpell(line);
            Assert.IsNull(guess);
            Assert.IsNotNull(player.UserCastingSpell);
        }

        [TestMethod]
        public void TestShamanUsingTashStick()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Tashan";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A rat glances nervously about.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Shaman
            };
            var guess = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, player.Player));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestWarriorDisciplineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "assumes an evasive fighting style.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, player);

            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClericAegolineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var aego = "Aegolism";
            var aegospell = spells.AllSpells.FirstOrDefault(a => a.name == aego);
            _ = spells.CastOtherSpells.TryGetValue(aegospell.cast_on_other, out var spells1);
            var player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, player);

            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClairityGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, player);
            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClairityDurationGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, player);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, player);
            Assert.IsNotNull(foundspell);
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestClairityDurationGuess1()
        {
            var spelllogparse = container.Resolve<SpellLogParse>();
            var line = "A soft breeze slips through your mind.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var spellmatch = spelllogparse.MatchSpell(line);

            Assert.IsNotNull(spellmatch);
        }

        [TestMethod]
        public void TestClairityDurationGuess_part1()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, player);
            var foundlevel = SpellDurations.MatchClosestLevelToSpell(foundspell, player);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, player);
            Assert.AreEqual(duration, 2100);
            Assert.AreEqual(foundlevel, 54);
        }

        [TestMethod]
        public void TestClairityDurationGuess_augmentDeath()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "You begin casting Augment Death.";
            var spellname = line.Substring(EQSpells.YouBeginCasting.Length - 1).Trim().TrimEnd('.');
            var player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Necromancer
            };
            if (spells.YouCastSpells.TryGetValue(spellname, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, player);
                Assert.IsNotNull(foundspell);
                Assert.AreEqual("Augment Death", foundspell.name);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestShieldOfWordsWIthNolevelOrclass()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Shield of Words.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            service.HandleYouBeginCastingSpellStart(line);
            var duration = SpellDurations.GetDuration_inSeconds(player.UserCastingSpell, player.Player);
            Assert.IsNotNull(player);
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestDeath()
        {
            var service = container.Resolve<LogDeathParse>();
            var line = "an ire Ghast has been slain by an ire ghast!";
            var targettoremove = service.GetDeadTarget(line);

            Assert.IsNotNull(targettoremove);
        }

        [TestMethod]
        public void GetCustomTimerStart()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Timer Start StupidGoblin 30'";
            var targettoremove = service.GetStartTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual(30 * 60, targettoremove.DurationInSeconds);
            Assert.AreEqual("stupidgoblin", targettoremove.Name);
        }

        [TestMethod]
        public void GetCustomTimerStart1()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Start Timer StupidGoblin 30'";
            var targettoremove = service.GetStartTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual(30 * 60, targettoremove.DurationInSeconds);
            Assert.AreEqual("stupidgoblin", targettoremove.Name);
        }

        [TestMethod]
        public void GetCustomTimerStart_TestSpaces()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Start Timer StupidGoblin with club near me 30'";
            var targettoremove = service.GetStartTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual(30 * 60, targettoremove.DurationInSeconds);
            Assert.AreEqual("stupidgoblin with club near me", targettoremove.Name);
        }

        [TestMethod]
        public void GetCustomTimerCancel()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Timer Cancel StupidGoblin'";
            var targettoremove = service.GetCancelTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual("stupidgoblin", targettoremove);
        }

        [TestMethod]
        public void GetCustomTimerCancel1()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Cancel Timer StupidGoblin'";
            var targettoremove = service.GetCancelTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual("stupidgoblin", targettoremove);
        }

        [TestMethod]
        public void GetCustomTimerCancel_TestSpaces()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "You say, 'Cancel Timer StupidGoblin with club near me'";
            var targettoremove = service.GetCancelTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual("stupidgoblin with club near me", targettoremove);
        }

        [TestMethod]
        public void TestLevelUpMatch()
        {
            var loger = container.Resolve<LevelLogParse>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            loger.MatchLevel("You have gained a level! Welcome to level 2!");
            Assert.AreEqual(2, player.Player.Level);

            loger.MatchLevel("You have gained a level! Welcome to level 60!");
            Assert.AreEqual(60, player.Player.Level);
        }

        [TestMethod]
        public void TestLevelUpMatch_NoPlayeryer_DoNoexplode()
        {
            var loger = container.Resolve<LevelLogParse>();
            _ = container.Resolve<ActivePlayer>();
            loger.MatchLevel("You have gained a level! Welcome to level 2!");
            loger.MatchLevel("You have gained a level! Welcome to level 60!");
        }

        [TestMethod]
        public void TestClassDetectionSpell1()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            service.HandleYouBeginCastingSpellStart(line);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }

        [TestMethod]
        public void TestClassDetectionSpell1_TestNulls()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            service.HandleYouBeginCastingSpellStart(line);
            Assert.IsNull(player.Player?.PlayerClass);
        }

        [TestMethod]
        public void TestLevelDetectionThroughSpells()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            service.HandleYouBeginCastingSpellStart(line);
            Assert.AreEqual(player.Player.Level, 60);
        }

        [TestMethod]
        public void DateParseTest()
        {
            var line = "Sat Oct 08 11:31:38 2022";
            var d = LogFileDateTimeParse.ParseDateTime(line);
            Assert.AreEqual(d.ToString(), "10/8/2022 11:31:38 AM");
        }

        [TestMethod]
        public void TashStickTest()
        {
            var logparser = container.Resolve<LogParser>();
            var spells = container.Resolve<EQSpells>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 60,
                PlayerClass = PlayerClasses.Shaman
            };
            var spellname = "Focus of Spirit";

            player.UserCastingSpell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spellnamefound = string.Empty;
            var line = "[Mon Jul 03 23:00:25 2023] TwentyTwo glances nervously about.";
            logparser.WhoEvent += (a, b) => Assert.Fail("DontHit");
            logparser.WhoPlayerEvent += (a, b) => Assert.Fail("DontHit");
            logparser.SpellWornOffSelfEvent += (a, b) => Assert.Fail("DontHit");
            logparser.SpellWornOtherOffEvent += (a, b) => Assert.Fail("DontHit");
            logparser.StartCastingEvent += (a, b) =>
            {
                spellnamefound = b.Spell.Spell.name;
            };
            logparser.CancelTimerEvent += (a, b) => Assert.Fail("DontHit");
            logparser.StartTimerEvent += (a, b) => Assert.Fail("DontHit");
            logparser.ConEvent += (a, b) => Assert.Fail("DontHit");
            logparser.DeadEvent += (a, b) => Assert.Fail("DontHit");
            logparser.FightHitEvent += (a, b) => Assert.Fail("DontHit");
            logparser.PlayerZonedEvent += (a, b) => Assert.Fail("DontHit");
            logparser.PlayerLocationEvent += (a, b) => Assert.Fail("DontHit");
            logparser.Push(line);
            Assert.AreEqual(spellnamefound, "Tashanian");
        }
    }
}
