using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQToolTests
{
    [TestClass]
    public class PlayerCommsTests
    {
        private readonly IContainer container;
        private readonly PlayerCommsParser parser;
        private readonly ActivePlayer activePlayer;

        public PlayerCommsTests()
        {
            container = DI.Init();
            parser = container.Resolve<PlayerCommsParser>();

            // fake in an ActivePlayer for the parser to be happy
            activePlayer = container.Resolve<ActivePlayer>();
            activePlayer.Player = new PlayerInfo();
            activePlayer.Player.Name = "Azleep";
            activePlayer.Player.Level = 60;
            activePlayer.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            activePlayer.Player.Zone = "templeveeshan";
        }

        [TestMethod]
        public void TestTell1()
        {
            //You told Qdyil, 'not even sure'
            DateTime now = DateTime.Now;
            var message = "You told Qdyil, 'not even sure'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("Qdyil", match.Receiver);
            Assert.AreEqual("not even sure", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestTell2()
        {
            //Azleep -> Jamori: ok
            DateTime now = DateTime.Now;
            var message = "Azleep -> Jamori: ok";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("Jamori", match.Receiver);
            Assert.AreEqual("ok", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestSay()
        {
            //You say, 'Hail, Wenglawks Kkeak'
            DateTime now = DateTime.Now;
            var message = "You say, 'Hail, Wenglawks Kkeak'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("Hail, Wenglawks Kkeak", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.SAY, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGroup()
        {
            //You tell your party, 'oh interesting'
            DateTime now = DateTime.Now;
            var message = "You tell your party, 'oh interesting'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("oh interesting", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.GROUP, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGuild()
        {
            //You say to your guild, 'nice'
            DateTime now = DateTime.Now;
            var message = "You say to your guild, 'nice'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("nice", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.GUILD, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestAuction()
        {
            //You auction, 'wtb diamond'
            DateTime now = DateTime.Now;
            var message = "You auction, 'wtb diamond'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("wtb diamond", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.AUCTION, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestOOC()
        {
            //You say out of character, 'train to west'
            DateTime now = DateTime.Now;
            var message = "You say out of character, 'train to west'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("train to west", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.OOC, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestShout()
        {
            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            DateTime now = DateTime.Now;
            var message = "You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("When it is time - Horse Charmers will be Leffingwell and Ceous", match.Content);
            Assert.AreEqual(PlayerCommsEvent.Channel.SHOUT, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }
    }
}
