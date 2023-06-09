using Autofac;
using EQTool.Models;
using EQTool.Services.Spells.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class PlayerParsingTests
    {
        private readonly IContainer container;
        public PlayerParsingTests()
        {
            container = DI.Init();
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
            Assert.IsNull(guess.Class);
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
            Assert.IsNull(guess.Class);
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
            Assert.AreEqual(PlayerClasses.Cleric, guess.Class);
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
            Assert.AreEqual(PlayerClasses.Warrior, guess.Class);
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
            Assert.AreEqual(PlayerClasses.Bard, guess.Class);
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
            Assert.AreEqual(PlayerClasses.Bard, guess.Class);
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
            Assert.IsNull(guess.Class);
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
            Assert.IsNull(guess.Class);
        }
    }
}
