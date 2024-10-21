using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQToolTests
{
    [TestClass]
    public class CommsTests
    {
        private readonly IContainer container;
        private readonly CommsParser parser;
        private readonly ActivePlayer activePlayer;

        public CommsTests()
        {
            container = DI.Init();
            parser = container.Resolve<CommsParser>();

            // fake in an ActivePlayer for the parser to be happy
            activePlayer = container.Resolve<ActivePlayer>();
            activePlayer.Player = new PlayerInfo();
            activePlayer.Player.Name = "Azleep";
            activePlayer.Player.Level = 60;
            activePlayer.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            activePlayer.Player.Zone = "templeveeshan";
        }

        [TestMethod]
        public void TestTell_FromPlayer ()
        {
            //You told Qdyil, 'not even sure'
            DateTime now = DateTime.Now;
            var message = "You told Qdyil, 'not even sure'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("Qdyil", match.Receiver);
            Assert.AreEqual("not even sure", match.Content);
            Assert.AreEqual(CommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestTell_FromOthers()
        {
            //[Sat Mar 21 17:55:33 2020] a spectre tells you, 'Attacking a spectre Master.'
            DateTime now = DateTime.Now;
            var message = "a spectre tells you, 'Attacking a spectre Master.'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("a spectre", match.Sender);
            Assert.AreEqual("you", match.Receiver);
            Assert.AreEqual("Attacking a spectre Master.", match.Content);
            Assert.AreEqual(CommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }


        [TestMethod]
        public void TestTell2_FromPlayer()
        {
            //Azleep -> Jamori: ok
            DateTime now = DateTime.Now;
            var message = "Azleep -> Jamori: ok";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("Jamori", match.Receiver);
            Assert.AreEqual("ok", match.Content);
            Assert.AreEqual(CommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestTell2_FromOthers()
        {
            //[Thu Aug 18 14:31:48 2022] Berrma -> Azleep: ya just need someone to invite i believe
            DateTime now = DateTime.Now;
            var message = "Berrma -> Azleep: ya just need someone to invite i believe";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Berrma", match.Sender);
            Assert.AreEqual("You", match.Receiver);
            Assert.AreEqual("ya just need someone to invite i believe", match.Content);
            Assert.AreEqual(CommsEvent.Channel.TELL, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestSay_FromPlayer()
        {
            //You say, 'Hail, Wenglawks Kkeak'
            DateTime now = DateTime.Now;
            var message = "You say, 'Hail, Wenglawks Kkeak'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("Hail, Wenglawks Kkeak", match.Content);
            Assert.AreEqual(CommsEvent.Channel.SAY, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestSay_FromOthers()
        {
            //[Wed Nov 20 20:29:06 2019] Jaloy says, 'i am a new warrior'
            DateTime now = DateTime.Now;
            var message = "Jaloy says, 'i am a new warrior'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Jaloy", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("i am a new warrior", match.Content);
            Assert.AreEqual(CommsEvent.Channel.SAY, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGroup_FromPlayer()
        {
            //You tell your party, 'oh interesting'
            DateTime now = DateTime.Now;
            var message = "You tell your party, 'oh interesting'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("oh interesting", match.Content);
            Assert.AreEqual(CommsEvent.Channel.GROUP, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGroup_FromOthers()
        {
            //Jaloy tells the group, 'wiki says he can be in 1 of 2 locations'
            DateTime now = DateTime.Now;
            var message = "Jaloy tells the group, 'wiki says he can be in 1 of 2 locations'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Jaloy", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("wiki says he can be in 1 of 2 locations", match.Content);
            Assert.AreEqual(CommsEvent.Channel.GROUP, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGuild_FromPlayer()
        {
            //You say to your guild, 'nice'
            DateTime now = DateTime.Now;
            var message = "You say to your guild, 'nice'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("nice", match.Content);
            Assert.AreEqual(CommsEvent.Channel.GUILD, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestGuild_FromOthers()
        {
            //[Wed Oct 16 17:17:25 2024] Okeanos tells the guild, 'it literally says speedway but the  products inside the store are 7/11 branded '
            DateTime now = DateTime.Now;
            var message = "Okeanos tells the guild, 'it literally says speedway but the  products inside the store are 7/11 branded '";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Okeanos", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("it literally says speedway but the  products inside the store are 7/11 branded ", match.Content);
            Assert.AreEqual(CommsEvent.Channel.GUILD, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestAuction_FromPlayer()
        {
            //You auction, 'wtb diamond'
            DateTime now = DateTime.Now;
            var message = "You auction, 'wtb diamond'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("wtb diamond", match.Content);
            Assert.AreEqual(CommsEvent.Channel.AUCTION, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestAuction_FromOthers()
        {
            //[Mon Feb 22 14:40:47 2021] Mezzter auctions, 'WTS bone chips 7p per stack pst'
            DateTime now = DateTime.Now;
            var message = "Mezzter auctions, 'WTS bone chips 7p per stack pst'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Mezzter", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("WTS bone chips 7p per stack pst", match.Content);
            Assert.AreEqual(CommsEvent.Channel.AUCTION, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestOOC_FromPlayer()
        {
            //You say out of character, 'train to west'
            DateTime now = DateTime.Now;
            var message = "You say out of character, 'train to west'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("train to west", match.Content);
            Assert.AreEqual(CommsEvent.Channel.OOC, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestOOC_FromOthers()
        {
            //[Wed Nov 20 20:18:47 2019] Enudara says out of character, 'grats'
            DateTime now = DateTime.Now;
            var message = "Enudara says out of character, 'grats'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Enudara", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("grats", match.Content);
            Assert.AreEqual(CommsEvent.Channel.OOC, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestShout_FromPlayer()
        {
            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            DateTime now = DateTime.Now;
            var message = "You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("You", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("When it is time - Horse Charmers will be Leffingwell and Ceous", match.Content);
            Assert.AreEqual(CommsEvent.Channel.SHOUT, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void TestShout_FromOthers()
        {
            //[Sat Aug 22 18:54:17 2020] Fizzix shouts, 'ASSIST Fizzix on --- [ an essence tamer ]'
            DateTime now = DateTime.Now;
            var message = "Fizzix shouts, 'ASSIST Fizzix on --- [ an essence tamer ]'";
            var match = parser.Match(message, now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Fizzix", match.Sender);
            Assert.AreEqual("", match.Receiver);
            Assert.AreEqual("ASSIST Fizzix on --- [ an essence tamer ]", match.Content);
            Assert.AreEqual(CommsEvent.Channel.SHOUT, match.TheChannel);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

    }
}
