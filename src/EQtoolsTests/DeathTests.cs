using Autofac;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQToolTests
{
    [TestClass]
    public class DeathTests
    {
        private readonly IContainer container;
        public DeathTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void TestMethod1()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            var rv = service.check_for_death("Just some random line", timestamp);
            Assert.AreEqual(rv, false);
        }

        [TestMethod]
        public void TestMethod2()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            var rv = service.check_for_death("You have been slain", timestamp);
            Assert.AreEqual(rv, true);
        }

        [TestMethod]
        public void TestMethod3()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp.AddSeconds(40.0));
            service.check_for_death("You have been slain", timestamp.AddSeconds(80.0));

            var count = service.deathloop_response();
            Assert.AreEqual(count, 3);
        }

        [TestMethod]
        public void TestMethod4()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp.AddSeconds(40.0));
            service.check_for_death("You have been slain", timestamp.AddSeconds(130.0));

            var count = service.deathloop_response();
            Assert.AreEqual(count, 2);
        }


        [TestMethod]
        public void TestMethod5()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);

            service.check_not_afk("You slice Soandso for 100 points of damage");
            var count = service.deathloop_response();
            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void TestMethod6()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);

            service.check_not_afk("You begin casting");
            var count = service.deathloop_response();
            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void TestMethod7()
        {
            DateTime timestamp = new DateTime();

            var service = container.Resolve<DeathParser>();
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);
            service.check_for_death("You have been slain", timestamp);

            service.check_not_afk("You shout, something something");
            var count = service.deathloop_response();
            Assert.AreEqual(count, 0);
        }

    }
}
