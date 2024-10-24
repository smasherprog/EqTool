﻿using Autofac;
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
            var match = parser.Match(message, now);

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
            var match = parser.Match(message, now);

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
            var match = parser.Match(message, now);

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
            var match = parser.Match(message, now);

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
            var match = parser.Match(message, now);

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
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.AttackerName);
            Assert.AreEqual("pierce", match.DamageType);
            Assert.AreEqual("an Iksar outcast", match.TargetName);
            Assert.AreEqual(0, match.DamageDone);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
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

            vm.TryAdd(new DamageEvent(now, line, "test1", "Test", 44, ""));

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
            _ = parser.Match(message, DateTime.Now);
        }

        [TestMethod]
        public void TestGuildChatCopyAndPaste()
        {
            var message = "vasanle tells the guild, '[Sun Jul 10 21:05:30 2022] pigy was hit by non-melee for 1500 points of damage.  [Sun Jul 10 21:05:30 2022] pigy staggers.'";
            var match = parser.Match(message, DateTime.Now);
            Assert.IsNull(match);
        }

        [TestMethod]
        public void TestLevelDetectionThroughKick()
        {
            var message = "You backstab a kick for 56 points of damage.";
            _ = parser.Match(message, DateTime.Now);
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };

            Assert.IsNull(player.Player.PlayerClass);
        }
    }
}
