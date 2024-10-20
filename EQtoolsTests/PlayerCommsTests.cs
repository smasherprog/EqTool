using Autofac;
using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EQTool.Services.Parsing;
using System;
using EQTool.Models;


namespace EQToolTests
{
    [TestClass]
    public class PlayerCommsTests
    {
        private readonly IContainer container;
        private readonly LogEvents logEvents;

        PlayerCommsTests()
        {
            container = DI.Init();
            logEvents = container.Resolve<LogEvents>();
        }


        [TestMethod]
        public void TestTell1()
        {
            //You told Qdyil, 'not even sure'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You told Qdyil, 'not even sure'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.TELL, match.theChannel);
        }

        [TestMethod]
        public void TestTell2()
        {
            //Azleep -> Jamori: ok
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "Azleep -> Jamori: ok";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.TELL, match.theChannel);
        }

        [TestMethod]
        public void TestSay()
        {
            //You say, 'Hail, Wenglawks Kkeak'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You say, 'Hail, Wenglawks Kkeak'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.SAY, match.theChannel);
        }

        [TestMethod]
        public void TestGroup()
        {
            //You tell your party, 'oh interesting'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You tell your party, 'oh interesting'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.GROUP, match.theChannel);
        }

        [TestMethod]
        public void TestGuild()
        {
            //You say to your guild, 'nice'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You say to your guild, 'nice'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.GUILD, match.theChannel);
        }

        [TestMethod]
        public void TestAuction()
        {
            //You auction, 'wtb diamond'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You auction, 'wtb diamond'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.AUCTION, match.theChannel);
        }

        [TestMethod]
        public void TestOOC()
        {
            //You say out of character, 'train to west'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You say out of character, 'train to west'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.OOC, match.theChannel);
        }

        [TestMethod]
        public void TestShout()
        {
            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            var commsParser = container.Resolve<PlayerCommsParser>();
            DateTime now = DateTime.Now;
            var message = "You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'";
            var match = commsParser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual(PlayerCommsEvent.Channel.SHOUT, match.theChannel);
        }
    }
}
