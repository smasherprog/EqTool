using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class PlayerLevelDetectionParserTests : BaseTestClass
    {
        private readonly LogParser logParser;

        public PlayerLevelDetectionParserTests()
        {
            logParser = container.Resolve<LogParser>();
            player.Player = new PlayerInfo { };
        }

        [TestMethod]
        public void TestLevelUpMatch()
        {
            var line = "You have gained a level! Welcome to level 2!";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 2);
            line = "You have gained a level! Welcome to level 60!";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
        }

        [TestMethod]
        public void TestLevelClassDetectionThroughSpells()
        {
            _ = container.Resolve<EQSpells>();
            var line = "You have finished memorizing Aegolism.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }

        [TestMethod]
        public void TestLevelDetectionThroughBackstab()
        {
            var message = "You backstab a willowisp for 56 points of damage.";
            logParser.Push(message, DateTime.Now);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Rogue);
        }

    }
}
