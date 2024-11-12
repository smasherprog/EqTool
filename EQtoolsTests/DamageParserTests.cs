using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class DamageParserTests : BaseTestClass
    {
        private readonly DamageParser parser;
        public DamageParserTests()
        {
            parser = container.Resolve<DamageParser>();
        }

        [TestMethod]
        public void DamageLogParser_EatingDpsParseTest()
        {
            var now = DateTime.Now;
            var message = "Vebanab slices a willowisp for 56 points of damage.";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("Vebanab", match.AttackerName);
            Assert.AreEqual("slices", match.DamageType);
            Assert.AreEqual("a willowisp", match.TargetName);
            Assert.AreEqual(56, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_NonMelleTest()
        {
            var now = DateTime.Now;
            var message = "Ratman Rager was hit by non-melee for 45 points of damage.";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("non-melee", match.DamageType);
            Assert.AreEqual("Ratman Rager", match.TargetName);
            Assert.AreEqual(45, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_EatingDpsParseTest1()
        {
            var now = DateTime.Now;
            var message = "a willowisp slices Vebanab for 56 points of damage.";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("a willowisp", match.AttackerName);
            Assert.AreEqual("slices", match.DamageType);
            Assert.AreEqual("Vebanab", match.TargetName);
            Assert.AreEqual(56, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_EatingDpsParseTestYou()
        {
            var now = DateTime.Now;
            var message = "You crush a shadowed man for 1 point of damage.";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("crush", match.DamageType);
            Assert.AreEqual("a shadowed man", match.TargetName);
            Assert.AreEqual(1, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_EatingDpsParseTestYouGetHit()
        {
            var now = DateTime.Now;
            var message = "Guard Valon bashes YOU for 12 points of damage.";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("Guard Valon", match.AttackerName);
            Assert.AreEqual("bashes", match.DamageType);
            Assert.AreEqual("YOU", match.TargetName);
            Assert.AreEqual(12, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_EatingDpsParseTestMiss()
        {
            var now = DateTime.Now;
            var message = "You try to pierce an Iksar outcast, but miss!";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("pierce", match.DamageType);
            Assert.AreEqual("an Iksar outcast", match.TargetName);
            Assert.AreEqual(0, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void DamageLogParser_YouHit()
        {
            //You crush a giant wasp drone for 12 points of damage.
            var now = DateTime.Now;
            var message = "You crush a giant wasp drone for 12 points of damage.";
            var lineCount = 101;
            var match = parser.Match(message, now, lineCount);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("crush", match.DamageType);
            Assert.AreEqual("a giant wasp drone", match.TargetName);
            Assert.AreEqual(12, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
            Assert.AreEqual(lineCount, match.LineCounter);

        }

        [TestMethod]
        public void DamageLogParser_YouMiss()
        {
            // you miss
            //You try to pierce a lava basilisk, but miss!
            //You try to crush spectral keeper, but spectral keeper's magical skin absorbs the blow!
            //You try to crush a froglok fisherman, but a froglok fisherman dodges!
            //You try to slash an elephant, but an elephant parries!
            //You try to pierce a tar goo, but a tar goo ripostes!
            //You try to pierce an earth elemental, but an earth elemental is INVULNERABLE!
            var now = DateTime.Now;
            var message = "You try to pierce an earth elemental, but an earth elemental is INVULNERABLE!";
            var lineCount = 101;
            var match = parser.Match(message, now, lineCount);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("pierce", match.DamageType);
            Assert.AreEqual("an earth elemental", match.TargetName);
            Assert.AreEqual(0, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
            Assert.AreEqual(lineCount, match.LineCounter);
        }

        [TestMethod]
        public void DamageLogParser_OthersHit()
        {
            // others hit
            //Giber slashes rogue clockwork for 13 points of damage.
            var now = DateTime.Now;
            var message = "Giber slashes rogue clockwork for 13 points of damage.";
            var lineCount = 101;
            var match = parser.Match(message, now, lineCount);

            Assert.IsNotNull(match);
            Assert.AreEqual("Giber", match.AttackerName);
            Assert.AreEqual("slashes", match.DamageType);
            Assert.AreEqual("rogue clockwork", match.TargetName);
            Assert.AreEqual(13, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
            Assert.AreEqual(lineCount, match.LineCounter);
        }

        [TestMethod]
        public void DamageLogParser_OthersMiss()
        {
            //A greater dark bone tries to hit Lazyboi, but misses!
            //Froglok krup knight tries to crush YOU, but YOUR magical skin absorbs the blow!
            //An undead oblation tries to hit Briton, but Briton's magical skin absorbs the blow!
            //A carrion ghoul tries to hit Slowmow, but Slowmow dodges!
            //A skeletal monk tries to hit Rellikcam, but Rellikcam parries!
            //A dusty werebat tries to hit Aleseeker, but Aleseeker ripostes!
            //A crimson claw hatchling tries to hit Frigs, but Frigs is INVULNERABLE!
            var now = DateTime.Now;
            var message = "A greater dark bone tries to hit Lazyboi, but misses!";
            var lineCount = 101;
            var match = parser.Match(message, now, lineCount);

            Assert.IsNotNull(match);
            Assert.AreEqual("A greater dark bone", match.AttackerName);
            Assert.AreEqual("hit", match.DamageType);
            Assert.AreEqual("Lazyboi", match.TargetName);
            Assert.AreEqual(0, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
            Assert.AreEqual(lineCount, match.LineCounter);
        }

        [TestMethod]
        public void TestDPS()
        {
            var entity = new EntittyDPS()
            {
                StartTime = DateTime.Now.AddSeconds(-20)
            };
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
            entity.UpdateDps();
            Assert.AreEqual(6, entity.TrailingDamage);
            Assert.AreEqual(7, entity.TotalDamage);
            Assert.AreEqual(5, entity.TotalTwelveSecondDamage);
        }

        [TestMethod]
        public void TestDPS2()
        {
            var entity = new EntittyDPS()
            {
                StartTime = DateTime.Now.AddSeconds(-20)
            };
            entity.AddDamage(new EntittyDPS.DamagePerTime
            {
                Damage = 44,
                TimeStamp = DateTime.Now.AddSeconds(-1)
            });

            entity.UpdateDps();
            Assert.AreEqual(44, entity.TrailingDamage);
            Assert.AreEqual(44, entity.TotalDamage);
            Assert.AreEqual(44, entity.TotalTwelveSecondDamage);
        }

        [TestMethod]
        public void TestDPS3()
        {
            var vm = container.Resolve<DPSWindowViewModel>();
            var now = DateTime.Now.AddSeconds(-1);
            var line = "some line";

            vm.TryAdd(new DamageEvent
            {
                Line = line,
                TimeStamp = now,
                AttackerName = string.Empty,
                DamageType = "bash",
                DamageDone = 44,
                TargetName = "Test",
                LineCounter = 0
            });

            var entity = vm.EntityList.FirstOrDefault();
            Assert.AreEqual(44, entity.TrailingDamage);
            Assert.AreEqual(44, entity.TotalDamage);
            Assert.AreEqual(44, entity.TotalTwelveSecondDamage);
            Assert.AreEqual(44, entity.HighestHit);
        }

        [TestMethod]
        public void TestDPSColors()
        {
            var r = new EntittyDPS
            {
                TotalDamage = 100,
                TargetTotalDamage = 1000
            };

            Assert.AreEqual(r.PercentOfTotalDamage, 10);
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstabNullCheck()
        {
            var message = "You backstab a willowisp for 56 points of damage.";
            _ = parser.Match(message, DateTime.Now, 0);
        }

        [TestMethod]
        public void TestGuildChatCopyAndPaste()
        {
            var message = "vasanle tells the guild, '[Sun Jul 10 21:05:30 2022] pigy was hit by non-melee for 1500 points of damage.  [Sun Jul 10 21:05:30 2022] pigy staggers.'";
            var match = parser.Match(message, DateTime.Now, 0);
            Assert.IsNull(match);
        }

        [TestMethod]
        public void TestLevelDetectionThroughKick()
        {
            var message = "You backstab a snow dervish for 56 points of damage.";
            _ = parser.Match(message, DateTime.Now, 0);
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };

            Assert.IsNull(player.Player.PlayerClass);
        }
    }
}
