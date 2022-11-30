using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
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
        public void TestSpellMatchCorrectlynecro1_GrimAura_wierd_but_Shouldhandle()
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
            var guesss = service.HandleBestGuessSpell(aegospell.cast_on_other);

            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
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
            var guesss = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other);

            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
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
            var guesss = service.HandleBestGuessSpell("Jobob " + spell.cast_on_other);

            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
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
            var guesss = service.HandleBestGuessSpell(shissarspell.cast_on_other);

            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar1()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "[Mon Nov 14 20:11:25 2022] Jobober" + shissarspell.cast_on_other;
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guesss = spelllogparse.MatchSpell(line);

            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
        }

        [TestMethod]
        public void TestSpeedOfShissar2()
        {
            var spells = container.Resolve<EQSpells>();
            var spelllogparse = container.Resolve<SpellLogParse>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "[Sun Nov 27 10:54:46 2022] A bottomless feaster's body pulses with the spirit of the Shissar.";
            var service = container.Resolve<ParseSpellGuess>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var guesss = spelllogparse.MatchSpell(line);
            var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(guesss.Spell, player.Player));
            Assert.IsNotNull(guesss);
            Assert.IsFalse(guesss.MutipleMatchesFound);
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
            var line = "[Mon Nov 14 20:11:25 2022] A soft breeze slips through your mind.";
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
        public void TestClairityDurationGuess_AgumentDeath()
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
        public void DPSLogParse_EatingDpsParseTest()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] Vebanab slices a willowisp for 56 points of damage.";
            var match = dpslogparse.Match(line);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.TimeStamp.ToString(), "11/14/2022 8:11:25 PM");
            Assert.AreEqual(match.SourceName, "Vebanab");
            Assert.AreEqual(match.TargetName, "a willowisp");
            Assert.AreEqual(match.DamageDone, 56);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTest1()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] a willowisp slices Vebanab for 56 points of damage.";
            var match = dpslogparse.Match(line);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "a willowisp");
            Assert.AreEqual(match.TargetName, "Vebanab");
            Assert.AreEqual(match.DamageDone, 56);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTestYou()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] You crush a shadowed man for 1 point of damage.";
            var match = dpslogparse.Match(line);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "You");
            Assert.AreEqual(match.TargetName, "a shadowed man");
            Assert.AreEqual(match.DamageDone, 1);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTestYouGetHit()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] Guard Valon bashes YOU for 12 points of damage.";
            var match = dpslogparse.Match(line);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "Guard Valon");
            Assert.AreEqual(match.TargetName, "YOU");
            Assert.AreEqual(match.DamageDone, 12);
        }

        [TestMethod]
        public void TestDeath()
        {
            var service = container.Resolve<LogDeathParse>();
            var line = "[Mon Nov 14 20:11:25 2022] an ire Ghast has been slain by an ire ghast!";
            var targettoremove = service.GetDeadTarget(line);

            Assert.IsNotNull(targettoremove);
        }

        [TestMethod]
        public void GetCustomTimerStart()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "[Mon Nov 14 20:11:25 2022] you say, 'Timer Start StupidGoblin 30'";
            var targettoremove = service.GetStartTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual(30 * 60, targettoremove.DurationInSeconds);
        }

        [TestMethod]
        public void GetCustomTimerStart1()
        {
            var service = container.Resolve<LogCustomTimer>();
            var line = "[Mon Nov 14 20:11:25 2022] you say, 'Start Timer StupidGoblin 30'";
            var targettoremove = service.GetStartTimer(line);

            Assert.IsNotNull(targettoremove);
            Assert.AreEqual(30 * 60, targettoremove.DurationInSeconds);
        }

        [TestMethod]
        public void TestDPS()
        {
            var entity = new EntittyDPS();
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-20)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-5)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-4)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-3)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-2)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(-1)
            });
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 1,
                TimeStamp = DateTime.Now.AddSeconds(10)
            });
            Assert.AreEqual(6, entity.TrailingDamage);
            Assert.AreEqual(7, entity.TotalDamage);
            Assert.AreEqual(5, entity.TotalTwelveSecondDamage);
        }
    }
}
