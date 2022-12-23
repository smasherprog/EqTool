using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var guesss = service.HandleBestGuessSpell(line);

            Assert.IsNotNull(guesss);
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
    }
}
