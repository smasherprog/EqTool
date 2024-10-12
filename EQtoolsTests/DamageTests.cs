using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQToolShared.Enums;
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
            var message = "Vebanab slices a willowisp for 56 points of damage.";
            var match = dpslogparse.Match(message, DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "Vebanab");
            Assert.AreEqual(match.TargetName, "a willowisp");
            Assert.AreEqual(match.DamageDone, 56);
        }

        [TestMethod]
        public void DPSLogParse_DOTParseTest()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "a goblin warrior has taken 12 damage from your Choke.";
            var match = dpslogparse.Match(message, DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "You");
            Assert.AreEqual(match.TargetName, "a goblin warrior");
            Assert.AreEqual(match.DamageDone, 12);
        }

        [TestMethod]
        public void DPSLogParse_NonMelleTest()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "Ratman Rager was hit by non-melee for 45 points of damage.";
            var match = dpslogparse.Match(message, DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "You");
            Assert.AreEqual(match.TargetName, "Ratman Rager");
            Assert.AreEqual(match.DamageDone, 45);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTest1()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "a willowisp slices Vebanab for 56 points of damage.";
            var match = dpslogparse.Match(message, DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "a willowisp");
            Assert.AreEqual(match.TargetName, "Vebanab");
            Assert.AreEqual(match.DamageDone, 56);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTestYou()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "You crush a shadowed man for 1 point of damage.";
            var match = dpslogparse.Match(message, DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual(match.SourceName, "You");
            Assert.AreEqual(match.TargetName, "a shadowed man");
            Assert.AreEqual(match.DamageDone, 1);
        }

        [TestMethod]
        public void DPSLogParse_EatingDpsParseTestYouGetHit()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "Guard Valon bashes YOU for 12 points of damage.";
            var match = dpslogparse.Match(message, DateTime.Now);

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
            var level = loger.MatchLevel("You have gained a level! Welcome to level 2!");
            Assert.AreEqual(2, level);

            level = loger.MatchLevel("You have gained a level! Welcome to level 60!");
            Assert.AreEqual(60, level);
        }

        [TestMethod]
        public void TestLevelUpMatch_NoPlayeryer_DoNoexplode()
        {
            var loger = container.Resolve<LevelLogParse>();
            _ = container.Resolve<ActivePlayer>();
            _ = loger.MatchLevel("You have gained a level! Welcome to level 2!");
            _ = loger.MatchLevel("You have gained a level! Welcome to level 60!");
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstab()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "You backstab a willowisp for 56 points of damage.";

            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            _ = dpslogparse.Match(message, DateTime.Now);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Rogue);
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstabNullCheck()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "You backstab a willowisp for 56 points of damage.";
            _ = dpslogparse.Match(message, DateTime.Now);
        }

        [TestMethod]
        public void TestGuildChatCopyAndPaste()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "vasanle tells the guild, '[Sun Jul 10 21:05:30 2022] pigy was hit by non-melee for 1500 points of damage.  [Sun Jul 10 21:05:30 2022] pigy staggers.'";
            var match = dpslogparse.Match(message, DateTime.Now);
            Assert.IsNull(match);
        }

        [TestMethod]
        public void TestLevelDetectionThroughKick()
        {
            var dpslogparse = container.Resolve<DPSLogParse>();
            var message = "You backstab a kick for 56 points of damage.";
            _ = dpslogparse.Match(message, DateTime.Now);
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };

            Assert.IsNull(player.Player.PlayerClass);
        }
    }
}
