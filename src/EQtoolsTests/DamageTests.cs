using Autofac;
using EQTool.Models;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQToolTests
{
    [TestClass]
    public class DamageTests
    {
        private readonly IContainer container;
        public DamageTests()
        {
            container = DI.Init();
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

            vm.TryAdd(new DPSParseMatch
            {
                DamageDone = 44,
                SourceName = "Test",
                TargetName = "test1",
                TimeStamp = DateTime.Now.AddSeconds(-1)
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
        public void TestLevelUpMatch()
        {
            var loger = container.Resolve<LevelLogParse>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            loger.MatchLevel("[Thu Nov 24 15:32:13 2022] You have gained a level! Welcome to level 2!");
            Assert.AreEqual(2, player.Player.Level);

            loger.MatchLevel("[Thu Nov 24 15:32:13 2022] You have gained a level! Welcome to level 60!");
            Assert.AreEqual(60, player.Player.Level);
        }

        [TestMethod]
        public void TestLevelUpMatch_NoPlayeryer_DoNoexplode()
        {
            var loger = container.Resolve<LevelLogParse>();
            _ = container.Resolve<ActivePlayer>();
            loger.MatchLevel("[Thu Nov 24 15:32:13 2022] You have gained a level! Welcome to level 2!");
            loger.MatchLevel("[Thu Nov 24 15:32:13 2022] You have gained a level! Welcome to level 60!");
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstab()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] You backstab a willowisp for 56 points of damage.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            _ = dpslogparse.Match(line);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Rogue);
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstabNullCheck()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] You backstab a willowisp for 56 points of damage.";
            _ = dpslogparse.Match(line);
        }

        [TestMethod]
        public void TestLevelDetectionThroughKick()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var line = "[Mon Nov 14 20:11:25 2022] You kick a willowisp for 56 points of damage.";
            _ = dpslogparse.Match(line);
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };

            Assert.IsNull(player.Player.PlayerClass);
        }
    }
}
