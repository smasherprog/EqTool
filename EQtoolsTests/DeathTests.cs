using Autofac;
using EQTool.Services;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQToolTests
{
    // [TestClass]
    public class DeathTests
    {
        private readonly IContainer container;
        private readonly DeathLoopService deathLoopService;
        private readonly LogEvents logEvents;

        public DeathTests()
        {
            container = DI.Init();
            deathLoopService = container.Resolve<DeathLoopService>();
            logEvents = container.Resolve<LogEvents>();
        }

        [TestMethod]
        // has the player died
        public void TestMethod1()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            var rv = service.ParseDeath("Just some random line", timestamp);
            Assert.AreEqual(false, rv);
        }


        [TestMethod]
        // has the player died
        public void ExampleTest()
        {
            var timestamp = DateTime.Now;

            logEvents.Handle(new EQTool.Models.DeadEvent { Name = "YOU", TimeStamp = timestamp });
            timestamp = timestamp.AddSeconds(1);
            logEvents.Handle(new EQTool.Models.DamageEvent { DamageDone = 1, AttackerName = "A", TargetName = "B", TimeStamp = timestamp });
            timestamp = timestamp.AddSeconds(1);
            logEvents.Handle(new EQTool.Models.YouZonedEvent { ZoneName = "somezonedoesntmatter", TimeStamp = timestamp });
            _ = timestamp.AddSeconds(1);
            //add a bunch of other things to make sure all works well
            Assert.AreEqual(false, deathLoopService.IsDeathLooping);
        }

        [TestMethod]
        // has the player died
        public void TestMethod2()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            var rv = service.ParseDeath("You have been slain", timestamp);
            Assert.AreEqual(true, rv);
        }

        [TestMethod]
        // player has died multiple times
        public void TestMethod3()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp.AddSeconds(40.0));
            _ = service.ParseDeath("You have been slain", timestamp.AddSeconds(80.0));
            _ = service.ParseDeath("You have been slain", timestamp.AddSeconds(80.0));

            var count = service.DeathLoopResponse();
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        // player has died multiple times, but by the time the last death occurs, the first death has scrolled off the tracking list
        public void TestMethod4()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp.AddSeconds(40.0));
            _ = service.ParseDeath("You have been slain", timestamp.AddSeconds(130.0));

            var count = service.DeathLoopResponse();
            Assert.AreEqual(2, count);
        }


        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // melee
        public void TestMethod5()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);

            _ = service.ParseSignOfLife("You slice Soandso for 100 points of damage");
            var count = service.DeathLoopResponse();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // casting
        public void TestMethod6()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);

            _ = service.ParseSignOfLife("You begin casting");
            var count = service.DeathLoopResponse();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        // player has died multiple times, but then shows sign of life, so the death tracking list is purged
        // communicating
        public void TestMethod7()
        {
            var timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);
            _ = service.ParseDeath("You have been slain", timestamp);

            _ = service.ParseSignOfLife("You shout, something something");
            var count = service.DeathLoopResponse();
            Assert.AreEqual(0, count);
        }

    }
}
