using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace EQToolTests
{
    [TestClass]
    public class DeathTests
    {
        private readonly IContainer container;
        private readonly DeathParser deathParser;
        private readonly CommsParser playerCommsParser;
        private readonly DamageParser damageParser;
        private readonly SpellCastParser spellCastParser;

        private readonly DeathLoopService deathLoopService;
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;

        public DeathTests()
        {
            container = DI.Init();
            deathParser = container.Resolve<DeathParser>();
            playerCommsParser = container.Resolve<CommsParser>();
            damageParser = container.Resolve<DamageParser>();
            spellCastParser = container.Resolve<SpellCastParser>();

            deathLoopService = container.Resolve<DeathLoopService>();
            logEvents = container.Resolve<LogEvents>();

            // fake in an ActivePlayer for the comms parser to be happy
            activePlayer = container.Resolve<ActivePlayer>();
            activePlayer.Player = new PlayerInfo();
            activePlayer.Player.Name = "Azleep";
            activePlayer.Player.Level = 60;
            activePlayer.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            activePlayer.Player.Zone = "templeveeshan";
        }

        [TestMethod]
        public void NoMatch()
        {
            // no match
            DateTime now = DateTime.Now;
            var message = "some bogus line";
            var match = deathParser.Match(message, now);

            Assert.IsNull(match);
        }

        [TestMethod]
        public void HasBeenSlainBy()
        {
            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            DateTime now = DateTime.Now;
            var message = "a Tesch Mas Gnoll has been slain by Genartik!";
            DeathEvent deathEvent = deathParser.Match(message, now);

            Assert.IsNotNull(deathEvent);
            Assert.AreEqual(now, deathEvent.TimeStamp);
            Assert.AreEqual(message, deathEvent.Line);
            Assert.AreEqual("a Tesch Mas Gnoll", deathEvent.Victim);
            Assert.AreEqual("Genartik", deathEvent.Killer);
        }

        [TestMethod]
        public void YouHaveBeenSlainBy()
        {
            //[Fri Nov 08 19:39:57 2019] You have been slain by a brigand!
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            DeathEvent deathEvent = deathParser.Match(message, now);

            Assert.IsNotNull(deathEvent);
            Assert.AreEqual(now, deathEvent.TimeStamp);
            Assert.AreEqual(message, deathEvent.Line);
            Assert.AreEqual("You", deathEvent.Victim);
            Assert.AreEqual("a brigand", deathEvent.Killer);
        }

        [TestMethod]
        public void YouHaveSlain()
        {
            //[Mon Sep 16 14:21:24 2024] You have slain a Tesch Mas Gnoll!
            DateTime now = DateTime.Now;
            var message = "You have slain a Tesch Mas Gnoll!";
            DeathEvent deathEvent = deathParser.Match(message, now);

            Assert.IsNotNull(deathEvent);
            Assert.AreEqual(now, deathEvent.TimeStamp);
            Assert.AreEqual(message, deathEvent.Line);
            Assert.AreEqual("a Tesch Mas Gnoll", deathEvent.Victim);
            Assert.AreEqual("You", deathEvent.Killer);
        }

        [TestMethod]
        public void SomeoneDied()
        {
            //[Sat Jan 16 20:12:37 2021] a bile golem died.
            DateTime now = DateTime.Now;
            var message = "a bile golem died.";
            DeathEvent deathEvent = deathParser.Match(message, now);

            Assert.IsNotNull(deathEvent);
            Assert.AreEqual(now, deathEvent.TimeStamp);
            Assert.AreEqual(message, deathEvent.Line);
            Assert.AreEqual("a bile golem", deathEvent.Victim);
        }

        [TestMethod]
        // player has died multiple times
        public void DeathListScroll()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now.AddSeconds(80.0));
            deathParser.Handle(message, now.AddSeconds(12.0));
            deathParser.Handle(message, now.AddSeconds(150.0));

            // oldest should have scrolled off
            var count = deathLoopService.DeathCount();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        // player has died multiple times
        public void AlmostDeathLoop()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now.AddSeconds(10.0));
            deathParser.Handle(message, now.AddSeconds(20.0));

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        // player has died multiple times
        public void DeathLoop()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now.AddSeconds(10.0));
            deathParser.Handle(message, now.AddSeconds(20.0));
            deathParser.Handle(message, now.AddSeconds(30.0));
            deathParser.Handle(message, now.AddSeconds(40.0));

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(5, count);
        }


        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // melee
        public void PlayerActive_Melee()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);

            // melee
            damageParser.Handle("You slice a moose for 100 points of damage", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // comms
        public void PlayerActive_Comms()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);

            // comms
            playerCommsParser.Handle("You told Mom, 'Look no hands!'", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // casting
        public void PlayerActive_Casting()
        {
            DateTime now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);
            deathParser.Handle(message, now);

            // casting
            spellCastParser.Handle("You begin casting Huge_Fireball", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }


    }
}
