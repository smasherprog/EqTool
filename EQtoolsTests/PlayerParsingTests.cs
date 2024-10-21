using Autofac;
using EQTool.Services.Parsing;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class PlayerParsingTests : BaseTestClass
    { 
        public PlayerParsingTests()
        { 
        }

        [TestMethod]
        public void AnonTest()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[ANONYMOUS] Rundorig  <The Drift>");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Rundorig", guess.Name);
            Assert.AreEqual("The Drift", guess.GuildName);
            Assert.IsNull(guess.Level);
            Assert.IsNull(guess.PlayerClass);
        }

        [TestMethod]
        public void AFKAnonTest()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("AFK [ANONYMOUS] Rundorig  <The Drift>");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Rundorig", guess.Name);
            Assert.AreEqual("The Drift", guess.GuildName);
            Assert.IsNull(guess.Level);
            Assert.IsNull(guess.PlayerClass);
        }

        [TestMethod]
        public void ClericTest60()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[60 High Priest] Dany (High Elf) <The Drift>");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Dany", guess.Name);
            Assert.AreEqual("The Drift", guess.GuildName);
            Assert.AreEqual(60, guess.Level);
            Assert.AreEqual(PlayerClasses.Cleric, guess.PlayerClass);
        }

        [TestMethod]
        public void WarTest60()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[58 Myrmidon] Bdain (Barbarian) <The Drift> LFG");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Bdain", guess.Name);
            Assert.AreEqual("The Drift", guess.GuildName);
            Assert.AreEqual(58, guess.Level);
            Assert.AreEqual(PlayerClasses.Warrior, guess.PlayerClass);
        }

        [TestMethod]
        public void Level1Bard()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[1 Bard] Bdain (Barbarian) <The Drift> LFG");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Bdain", guess.Name);
            Assert.AreEqual("The Drift", guess.GuildName);
            Assert.AreEqual(1, guess.Level);
            Assert.AreEqual(PlayerClasses.Bard, guess.PlayerClass);
        }

        [TestMethod]
        public void Level1BardNoGuild()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[1 Bard] Bdain (Barbarian) LFG");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Bdain", guess.Name);
            Assert.IsNull(guess.GuildName);
            Assert.AreEqual(1, guess.Level);
            Assert.AreEqual(PlayerClasses.Bard, guess.PlayerClass);
        }

        [TestMethod]
        public void AllAnon()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[ANONYMOUS] Rundorig");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Rundorig", guess.Name);
            Assert.IsNull(guess.GuildName);
            Assert.IsNull(guess.Level);
            Assert.IsNull(guess.PlayerClass);
        }

        [TestMethod]
        public void AllAnonLfg()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("[ANONYMOUS] Rundorig  LFG");
            Assert.IsNotNull(guess);
            Assert.AreEqual("Rundorig", guess.Name);
            Assert.IsNull(guess.GuildName);
            Assert.IsNull(guess.Level);
            Assert.IsNull(guess.PlayerClass);
        }

        [TestMethod]
        public void GuildMOTD()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo("GUILD MOTD: <<>> things go here [ test ] ");
            Assert.IsNull(guess);
        }

        [TestMethod]
        public void TestRandomCharacters()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo(" [TESTER 10:00PM] [VIEW] bla bla bla bla bla ' ");
            Assert.IsNull(guess);
        }

        [TestMethod]
        public void TestRandomCharacters1()
        {
            var service = container.Resolve<PlayerWhoLogParse>();
            var guess = service.ParsePlayerInfo(" [ TESTER 10:00PM] [VIEW] bla bla bla bla bla ' ");
            Assert.IsNull(guess);
        }
    }
}
