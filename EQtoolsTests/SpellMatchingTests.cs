using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.Services.Parsing.Helpers;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellMatchingTests : BaseTestClass
    {
        public SpellMatchingTests()
        {
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
            var allkeys = Zones.ZoneInfoMap.Keys
                .Concat(Zones.ZoneNameMapper.Keys)
                 .Concat(Zones.ZoneWhoMapper.Keys)
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
            //var allkeys = Zones.NoZoneZHelping.Keys
            // .Concat(Zones.ZoneNameMapper.Keys)
            //  .Concat(Zones.ZoneWhoMapper.Keys)
            //  .Distinct()
            //  .ToList();
            //var list = new List<ZoneInfo>();
            //var notablenpc = "! ''' Notable NPCs: '''";
            //foreach (var item in allkeys)
            //{
            //    var zoneinfo = new ZoneInfo
            //    {
            //        Victim = item
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
            //    Debug.WriteLine(item.Victim);
            //    Debug.WriteLine(string.Join(",", item.NotableNPCs));
            //}
        }

        [TestMethod]
        public void TestParseP99GrimAura()
        {
            _ = container.Resolve<EQSpells>();
            var line = "8639^Grim Aura^PLAYER_1^^^^A dull aura covers your hand.^'s hand is covered with a dull aura.^The grim aura fades.^0^0^0^0^3000^2250^2250^3^270^0^25^3^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^10^0^0^0^0^0^0^0^0^0^0^0^2503^2108^-1^-1^-1^-1^1^1^1^1^-1^-1^-1^-1^102^100^100^100^100^100^100^100^100^100^100^100^0^1^0^0^2^254^254^254^254^254^254^254^254^254^254^254^6^25^5^-1^0^0^255^255^255^255^22^255^255^255^255^255^4^255^255^255^255^255^43^0^0^8^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^100^0^37^94^0^0^0^0^0^0^0^0^0^0^0^7^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^5^101^12^92^3^270^0^0^0^0^3^105^0^0^0^0^0^0^0^0^0^0^1^1^0^0^0^0^0^-1^0^0^0^1^0^0^1^1^^0";
            var grimaura = ParseSpells_spells_us.ParseP99Line(line);
            Assert.IsNotNull(grimaura);
        }


        [TestMethod]
        public void TestNPCSpellKlandicar1()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "You feel your skin freeze.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Silver Breath");
        }

        [TestMethod]
        public void TestNPCSpellKlandicar2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone's skin freezes.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Silver Breath");
        }

        [TestMethod]
        public void TestDragonRoar()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "You flee in terror.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Dragon Roar");
        }

        [TestMethod]
        public void TestDragonRoarResist()
        {
            var service = container.Resolve<ResistSpellParser>();
            var message = "You resist the Dragon Roar spell!";
            var spell = service.ParseNPCSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Dragon Roar");
        }

        [TestMethod]
        public void TestStunBreath1()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your eardrums rupture.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Stun Breath");
        }

        [TestMethod]
        public void TestStunBreath2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone staggers with intense pain.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Stun Breath");
        }

        [TestMethod]
        public void TestRottingFlesh()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your flesh begins to rot.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Rotting Flesh");
        }

        [TestMethod]
        public void TestRottingFlesh2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone's flesh begins to rot.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Rotting Flesh");
        }

        [TestMethod]
        public void TestPutrefyFlesh()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your flesh begins to liquefy.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Putrefy Flesh");
        }

        [TestMethod]
        public void TestPutrefyFlesh2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone's flesh begins to liquefy.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Putrefy Flesh");
        }

        [TestMethod]
        public void TestImmolatingBreath()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your flesh is seared from your bones.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Immolating Breath");
        }

        [TestMethod]
        public void TestAncientBreath()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your life force drains away.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Ancient Breath");
        }

        [TestMethod]
        public void TestImmolatingBreath2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone's flesh is seared.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Immolating Breath");
        }

        [TestMethod]
        public void TestWaveofCold()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "A blast of cold freezes your skin.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Wave of Cold");
        }

        [TestMethod]
        public void TestWaveofCold2()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Someone's skin freezes.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Silver Breath");
        }

        [TestMethod]
        public void TestRaidofMoltenLava()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Lava sears your skin.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Rain of Molten Lava");
        }

        [TestMethod]
        public void TestLavaBreath()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your body combusts as the lava hits you.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Lava Breath");
        }

        [TestMethod]
        public void TestFrostBreath()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your body freezes as the frost hits you.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Frost Breath");
        }

        [TestMethod]
        public void TestTsunami()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "A tsunami crushes you.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Tsunami");
        }

        [TestMethod]
        public void TestCloudofFear()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your mind is wracked by fear.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Cloud of Fear");
        }

        [TestMethod]
        public void TestDiseasedCloud()
        {
            var service = container.Resolve<SpellCastParser>();
            var message = "Your body begins to rot.";
            var spell = service.MatchSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Diseased Cloud");
        }

        [TestMethod]
        public void TestFrostBreathResist()
        {
            var service = container.Resolve<ResistSpellParser>();
            var message = "You resist the Frost Breath spell!";
            var spell = service.ParseNPCSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Frost Breath");
        }

        [TestMethod]
        public void TestLavaBreathResist()
        {
            var service = container.Resolve<ResistSpellParser>();
            var message = "You resist the Lava Breath spell!";
            var spell = service.ParseNPCSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Lava Breath");
        }

        [TestMethod]
        public void TestMoltenBreathResist()
        {
            var service = container.Resolve<ResistSpellParser>();
            var message = "You resist the Molten Breath spell!";
            var spell = service.ParseNPCSpell(message, DateTime.Now, 0);
            Assert.AreEqual(spell.Spell.name, "Molten Breath");
        }

        //[TestMethod]
        //public void TestCloudofDisempowerment()
        //{
        //    var service = container.Resolve<SpellLogParse>();
        //    var message = "You feel your skin freeze.";
        //    var spell = service.MatchSpell(message);
        //    Assert.AreEqual(spell.Spell.name, "Cloud of Disempowerment");
        //}

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_GrimAura()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.ShadowKnight, 30);
            Assert.AreEqual(ret, 30);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Journeymansboots()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "JourneymanBoots";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(spell, PlayerClasses.ShadowKnight, 35);
            Assert.AreEqual(ret, 35);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Alliance()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Alliance";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other);
            var duration = SpellDurations.GetDuration_inSeconds(spell, PlayerClasses.ShadowKnight, 35);
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
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.ShadowKnight, 60);
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk60_NaltronMark()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Naltron's Mark";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other).ToList();
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(spell1, PlayerClasses.ShadowKnight, 60);
            Assert.AreEqual(ret.name, "Symbol of Naltron");
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro1_GrimAura_weird_but_Shouldhandle()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.Necromancer, 1);
            Assert.AreEqual(ret, 4);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro60_GrimAura()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.Necromancer, 60);
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectly_Level60_GrimAura_NoClass()
        {
            var spells = container.Resolve<EQSpells>();
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, null, 60);
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestWarriorDiscipline()
        {
            _ = container.Resolve<EQSpells>();
            var line = "jobob assumes an evasive fighting style.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;
            var guess = service.HandleBestGuessSpell(line, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell(line, DateTime.Now, 0);

            Assert.IsNotNull(guess);
        }

        [TestMethod]
        public void TestBlackPomFlower()
        {
            _ = container.Resolve<EQSpells>();
            var line = "Someone is covered by an aura of black petals.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Enchanter
            };
            var guess = service.HandleBestGuessSpell(line, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell(aegospell.cast_on_other, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other, DateTime.Now, 0);

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
            var guess = service.HandleBestGuessSpell(shissarspell.cast_on_other, DateTime.Now, 0);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
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
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);

            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForShadowKnight()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Shaman, 60));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForMagePetHaste()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "You begin casting Burnout.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 21,
                PlayerClass = PlayerClasses.Magician
            };
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            Assert.AreEqual(shissarspell, player.UserCastingSpell);
        }

        [TestMethod]
        public void TestUserBeginsToCastUserCastingSpell()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = $"You begin casting {spellname}.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 21,
                PlayerClass = PlayerClasses.Magician
            };
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            Assert.AreEqual(shissarspell, player.UserCastingSpell);
            line = "Jobober " + shissarspell.cast_on_other;
            guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            Assert.IsTrue(guess.CastByYou);
        }

        [TestMethod]
        public void TestPrimalEssence()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "You begin casting Primal Essence.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 55,
                PlayerClass = PlayerClasses.Shaman
            };
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
        }

        [TestMethod]
        public void TestSlowForMagePetHaste1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 21;
            player.Player.PlayerClass = PlayerClasses.Magician;

            player.UserCastingSpell = shissarspell;
            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Magician, 21));
            Assert.AreEqual(15, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestFluxStaff()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "LowerElement";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + spell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 52;
            player.Player.PlayerClass = PlayerClasses.Warrior;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Warrior, 52));
            Assert.AreEqual(5, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_onespace()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
            Assert.AreEqual(6, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.AreEqual(guess.TargetName, "an Jobober");
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestManicalStrength()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Manicial Strength";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Shaman, 60));
            Assert.AreEqual(144, spellduration.TotalMinutes);
            Assert.IsNotNull(guess);
            Assert.AreEqual(guess.Spell.name, spellname);
            Assert.AreEqual(guess.TargetName, "an Jobober");
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_twospace()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
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
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Cripple";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
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
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A bottomless feaster's body pulses with the spirit of the Shissar.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Cleric, 54));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A Ratling is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic2()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Gkrean Prophet of Tallon is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
            Assert.IsNotNull(guess);
            Assert.AreEqual("Gkrean Prophet of Tallon", guess.TargetName);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanEpic1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A rat Ratling is consumed by the raging spirits of the land.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Necromancer, 60));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestShamanAcumen()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var spellname = "Acumen";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = EQSpells.YouBeginCasting + " " + spellname;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            Assert.IsNull(guess);
            Assert.IsNotNull(player.UserCastingSpell);
        }

        [TestMethod]
        public void TestShamanUsingTashStick()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellCastParser>();
            var shissar = "Tashan";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A rat glances nervously about.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;

            var guess = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guess.Spell, PlayerClasses.Shaman, 60));
            Assert.IsNotNull(guess);
            Assert.IsFalse(guess.MultipleMatchesFound);
        }

        [TestMethod]
        public void TestWarriorDisciplineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "assumes an evasive fighting style.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, PlayerClasses.Cleric, 54);

            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClericAegolineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var aego = "Aegolism";
            var aegospell = spells.AllSpells.FirstOrDefault(a => a.name == aego);
            _ = spells.CastOtherSpells.TryGetValue(aegospell.cast_on_other, out var spells1);
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, PlayerClasses.Cleric, 54);

            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClairityGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, PlayerClasses.Cleric, 54);
            Assert.IsNotNull(foundspell);
        }

        [TestMethod]
        public void TestClairityDurationGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, PlayerClasses.Cleric, 54);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, PlayerClasses.Cleric, 54);
            Assert.IsNotNull(foundspell);
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestClairityDurationGuess1()
        {
            var spelllogparse = container.Resolve<SpellCastParser>();
            var line = "A soft breeze slips through your mind.";
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;

            var spellmatch = spelllogparse.MatchSpell(line, DateTime.Now, 0);

            Assert.IsNotNull(spellmatch);
        }

        [TestMethod]
        public void TestHealForDamage()
        {
            var spelllogparse = container.Resolve<SpellCastParser>();
            var line = "You mend your wounds and heal some damage.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var spellmatch = spelllogparse.MatchSpell(line, DateTime.Now, 0);
            Assert.IsNotNull(spellmatch);
        }

        [TestMethod]
        public void TestClairityDurationGuess_part1()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            var foundspell = SpellDurations.MatchClosestLevelToSpell(spells1, PlayerClasses.Cleric, 54);
            var foundlevel = SpellDurations.MatchClosestLevelToSpell(foundspell, PlayerClasses.Cleric, 54);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, PlayerClasses.Cleric, 54);
            Assert.AreEqual(duration, 2100);
            Assert.AreEqual(foundlevel, 54);
        }

        [TestMethod]
        public void TestClairityDurationGuess_augmentDeath()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "You begin casting Augment Death.";
            var spellname = line.Substring(EQSpells.YouBeginCasting.Length - 1).Trim().TrimEnd('.');
            if (spells.YouCastSpells.TryGetValue(spellname, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, PlayerClasses.Necromancer, 60);
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
            player.Player.Level = 0;
            player.Player.PlayerClass = null;
            service.HandleYouBeginCastingSpellStart(line, DateTime.Now, 0);
            var duration = SpellDurations.GetDuration_inSeconds(player.UserCastingSpell, null, null);
            Assert.IsNotNull(player);
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestDeath()
        {
            var now = DateTime.Now;
            var service = container.Resolve<SlainParser>();
            var line = "an ire Ghast has been slain by a different ire ghast!";
            var de = service.Match(line, now, 0);

            Assert.IsNotNull(de);
            Assert.AreEqual(now, de.TimeStamp);
            Assert.AreEqual(line, de.Line);
            Assert.AreEqual("an ire Ghast", de.Victim);
            Assert.AreEqual("a different ire ghast", de.Killer);
        }

        [TestMethod]
        public void TestDeath1()
        {
            var now = DateTime.Now;
            var service = container.Resolve<SlainParser>();
            var line = "Harbinger Freglor has been slain by skeletal champion!";
            var de = service.Match(line, now, 0);

            Assert.IsNotNull(de);
            Assert.AreEqual(now, de.TimeStamp);
            Assert.AreEqual(line, de.Line);
            Assert.AreEqual("Harbinger Freglor", de.Victim);
            Assert.AreEqual("skeletal champion", de.Killer);
        }

        [TestMethod]
        public void TestDeath2()
        {
            var now = DateTime.Now;
            var service = container.Resolve<SlainParser>();
            var line = "You have slain Arch Duke Iatol!";
            var de = service.Match(line, now, 0);

            Assert.IsNotNull(de);
            Assert.AreEqual(now, de.TimeStamp);
            Assert.AreEqual(line, de.Line);
            Assert.AreEqual("Arch Duke Iatol", de.Victim);
            Assert.AreEqual("You", de.Killer);
        }

        [TestMethod]
        public void TestDeathByDot()
        {
            var now = DateTime.Now;
            var service = container.Resolve<SlainParser>();
            var line = "an ire Ghast died.";
            var de = service.Match(line, now, 0);

            Assert.IsNotNull(de);
            Assert.AreEqual(now, de.TimeStamp);
            Assert.AreEqual(line, de.Line);
            Assert.AreEqual("an ire Ghast", de.Victim);
        }

        [TestMethod]
        public void TestClassDetectionSpell1_TestNulls()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            service.HandleYouBeginCastingSpellStart(line, DateTime.Now, 0);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }


        [TestMethod]
        public void DateParseTest()
        {
            var line = "Sat Oct 08 11:31:38 2022";
            var d = LogFileDateTimeParse.ParseDateTime(line);
            Assert.AreEqual(d.ToString("M'/'d'/'yyyy hh:mm:ss tt"), "10/8/2022 11:31:38 AM");
        }

        [TestMethod]
        public void TashStickTest()
        {
            var logparser = container.Resolve<LogParser>();
            var logevents = container.Resolve<LogEvents>();
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
            var line = "TwentyTwo glances nervously about.";
            logevents.WhoEvent += (a, b) => Assert.Fail("DontHit");
            logevents.WhoPlayerEvent += (a, b) => Assert.Fail("DontHit");
            logevents.SpellWornOffSelfEvent += (a, b) => Assert.Fail("DontHit");
            logevents.SpellWornOffOtherEvent += (a, b) => Assert.Fail("DontHit");
            logevents.SpellCastEvent += (a, b) =>
            {
                spellnamefound = b.Spell.name;
            };
            logevents.ConEvent += (a, b) => Assert.Fail("DontHit");
            logevents.SlainEvent += (a, b) => Assert.Fail("DontHit");
            logevents.DamageEvent += (a, b) => Assert.Fail("DontHit");
            logevents.YouZonedEvent += (a, b) => Assert.Fail("DontHit");
            logevents.PlayerLocationEvent += (a, b) => Assert.Fail("DontHit");
            logparser.Push(line, DateTime.Now);
            Assert.AreEqual(spellnamefound, "Tashanian");
        }

        //[TestMethod]
        //public void ParseItemListFromQuarm()
        //{
        //    var data = File.ReadAllText("C:\\Users\\smash\\Downloads\\itemslist.txt");
        //    var splits = data.Split(new string[] { "),(" }, StringSplitOptions.None);
        //    splits[0] = splits[0].Trim('(');
        //    splits[splits.Length - 1] = splits[splits.Length - 1].Trim(')');

        //    var outputstring = "{";
        //    foreach (var item in splits)
        //    {
        //        var innersplits = item.Split(',');
        //        var name = innersplits[2].Replace("\\", string.Empty).Trim('\'');
        //        outputstring += "\"" + name + "\",";
        //    }
        //    outputstring.TrimEnd(',');
        //    outputstring += "}";
        //    //Debug.WriteLine(outputstring);
        //    File.WriteAllText("C:\\Users\\smash\\Downloads\\itemslist1.txt", outputstring);
        //    Assert.IsTrue(splits.Any());
        //}

        //[TestMethod]
        //public void ParseItemNPCFromQuarm()
        //{
        //    var data = System.IO.File.ReadAllText("C:\\Users\\smash\\Downloads\\quarm_2023-12-13-19_15\\quarm_2023-12-13-19_15.sql");
        //    var splits = data.Split(new string[] { "),(" }, StringSplitOptions.None);
        //    splits[0] = splits[0].Trim('(');
        //    splits[splits.Length - 1] = splits[splits.Length - 1].Trim(')');

        //    var outputstring = string.Empty;
        //    foreach (var item in splits)
        //    {
        //        var innersplits = item.Split(',');
        //        var name = innersplits[1].Replace("\\", string.Empty).Trim('\'');
        //        if (name.StartsWith("_"))
        //        {
        //            continue;
        //        }
        //        name = name.Trim('#');
        //        name = name.Replace("_", " ");
        //        outputstring += "\"" + name + "\",";
        //    }
        //    outputstring.TrimEnd(',');
        //    //Debug.WriteLine(outputstring);
        //    System.IO.File.WriteAllText("C:\\Users\\smash\\Downloads\\npclist.txt", outputstring);
        //    Assert.IsTrue(splits.Any());
        //}
    }
}
