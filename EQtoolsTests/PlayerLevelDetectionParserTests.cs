using Autofac;
using EQTool.Models;
using EQTool.Services.Handlers;
using EQTool.Services.Parsing;
using EQTool.Services.Parsing.Helpers;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EQtoolsTests
{
    [TestClass]
    public class PlayerLevelDetectionParserTests : BaseTestClass
    {
        public PlayerLevelDetectionParserTests()
        {
            _ = container.Resolve<IEnumerable<BaseHandler>>();
        }

        [TestMethod]
        public void TestLevelUpMatch()
        {
            var loger = container.Resolve<PlayerLevelDetectionParser>();
            var level = loger.MatchLevel("You have gained a level! Welcome to level 2!");
            Assert.AreEqual(2, level);

            level = loger.MatchLevel("You have gained a level! Welcome to level 60!");
            Assert.AreEqual(60, level);
        }

        [TestMethod]
        public void TestLevelUpMatch_NoPlayeryer_DoNoexplode()
        {
            var loger = container.Resolve<PlayerLevelDetectionParser>();
            _ = loger.MatchLevel("You have gained a level! Welcome to level 2!");
            _ = loger.MatchLevel("You have gained a level! Welcome to level 60!");
        }

        [TestMethod]
        public void TestLevelDetectionThroughSpells()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            service.HandleYouBeginCastingSpellStart(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
        }

        [TestMethod]
        public void TestClassDetectionSpell()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You begin casting Aegolism.";
            var service = container.Resolve<ParseHandleYouCasting>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            service.HandleYouBeginCastingSpellStart(line, DateTime.Now);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstab()
        {
            var message = "You backstab a willowisp for 56 points of damage.";

            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo { };
            _ = container.Resolve<DamageParser>().Match(message, DateTime.Now);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Rogue);
        }

    }
}
