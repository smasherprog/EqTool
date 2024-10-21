using Autofac;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{

    [TestClass]
    public class DeathTests : BaseTestClass
    {
        private readonly DeathParserNew deathParser;
        private readonly CommsParser playerCommsParser;
        private readonly DamageParser damageParser;
        private readonly SpellCastParser spellCastParser;
        private readonly DeathLoopService deathLoopService;
        private readonly LogEvents logEvents;

        public DeathTests()
        {
            deathParser = container.Resolve<DeathParserNew>();
            playerCommsParser = container.Resolve<CommsParser>();
            damageParser = container.Resolve<DamageParser>();
            spellCastParser = container.Resolve<SpellCastParser>();

            deathLoopService = container.Resolve<DeathLoopService>();
            logEvents = container.Resolve<LogEvents>();


            var activePlayer = container.Resolve<ActivePlayer>();
            activePlayer.Player.Name = "Azleep";
            activePlayer.Player.Level = 60;
            activePlayer.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            activePlayer.Player.Zone = "templeveeshan";
        }

        [TestMethod]
        public void NoMatch()
        {
            // no match
            var now = DateTime.Now;
            var message = "some bogus line";
            var match = deathParser.Match(message, now);

            Assert.IsNull(match);
        }

        [TestMethod]
        public void HasBeenSlainBy()
        {
            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            var now = DateTime.Now;
            var message = "a Tesch Mas Gnoll has been slain by Genartik!";
            var deathEvent = deathParser.Match(message, now);

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
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            var deathEvent = deathParser.Match(message, now);

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
            var now = DateTime.Now;
            var message = "You have slain a Tesch Mas Gnoll!";
            var deathEvent = deathParser.Match(message, now);

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
            var now = DateTime.Now;
            var message = "a bile golem died.";
            var deathEvent = deathParser.Match(message, now);

            Assert.IsNotNull(deathEvent);
            Assert.AreEqual(now, deathEvent.TimeStamp);
            Assert.AreEqual(message, deathEvent.Line);
            Assert.AreEqual("a bile golem", deathEvent.Victim);
        }

        [TestMethod]
        // player has died multiple times
        public void DeathListScroll()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now.AddSeconds(80.0));
            _ = deathParser.Handle(message, now.AddSeconds(12.0));
            _ = deathParser.Handle(message, now.AddSeconds(150.0));

            // oldest should have scrolled off
            var count = deathLoopService.DeathCount();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        // player has died multiple times
        public void AlmostDeathLoop()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now.AddSeconds(10.0));
            _ = deathParser.Handle(message, now.AddSeconds(20.0));

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        // player has died multiple times
        public void DeathLoop()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now.AddSeconds(10.0));
            _ = deathParser.Handle(message, now.AddSeconds(20.0));
            _ = deathParser.Handle(message, now.AddSeconds(30.0));
            _ = deathParser.Handle(message, now.AddSeconds(40.0));

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(5, count);
        }


        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // melee
        public void PlayerActive_Melee()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);

            // melee
            _ = damageParser.Handle("You slice a moose for 100 points of damage", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // comms
        public void PlayerActive_Comms()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);

            // comms
            _ = playerCommsParser.Handle("You told Mom, 'Look no hands!'", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // casting
        public void PlayerActive_Casting()
        {
            var now = DateTime.Now;
            var message = "You have been slain by a brigand!";
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);
            _ = deathParser.Handle(message, now);

            // casting
            _ = spellCastParser.Handle("You begin casting Huge_Fireball", now);

            var count = deathLoopService.DeathCount();
            Assert.AreEqual(0, count);
        }


    }
}
