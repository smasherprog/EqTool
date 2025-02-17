using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class PlayerLevelDetectionParserTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private const string YouBeginCasting = "You begin casting ";
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
            var line = "You have finished memorizing Aegolism.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }

        [TestMethod]
        public void TestLevelClassDetectionThroughCasting()
        {
            var spells = container.Resolve<EQSpells>();
            var s = spells.AllSpells.FirstOrDefault(a => a.name == "Aegolism"); 
            var line = YouBeginCasting + "Aegolism";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Cleric);
        }

        [TestMethod]
        public void TestLevelClassDetectionThroughCasting_DemiLich()
        {
            var spells = container.Resolve<EQSpells>();
            var s = spells.AllSpells.FirstOrDefault(a => a.name == "Demi Lich");
            var line = YouBeginCasting + "Demi Lich";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 60);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Necromancer);
        }

        [TestMethod]
        public void TestLevelClassDetectionThroughCasting_EnchIllusion()
        {
            var spells = container.Resolve<EQSpells>();
            var s = spells.AllSpells.FirstOrDefault(a => a.name == "Illusion: Dark Elf");
            var line = YouBeginCasting + "Shallow Breath"; 
            logParser.Push(line, DateTime.Now);
            line = YouBeginCasting + "Illusion: Dark Elf"; 
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual(player.Player.Level, 12);
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Enchanter);
        }

        [TestMethod]
        public void TestLevelClassDetectionThroughCasting_EnchIllusion_ClassWarrior()
        {
            var spells = container.Resolve<EQSpells>();
            var s = spells.AllSpells.FirstOrDefault(a => a.name == "Illusion: Dark Elf");
            var line = YouBeginCasting + "Illusion: Dark Elf";
            player.Player.PlayerClass = PlayerClasses.Warrior;
            logParser.Push(line, DateTime.Now); 
            Assert.AreEqual(player.Player.PlayerClass, PlayerClasses.Warrior);
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
