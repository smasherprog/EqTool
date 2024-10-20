using Autofac;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQToolTests
{
    [TestClass]
    public class RandomRollTests
    {
        private readonly IContainer container;
        public RandomRollTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void RandomParseTests()
        {
            var service = container.Resolve<RandomParser>();
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.", DateTime.Now);
            Assert.IsNull(match);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.", DateTime.Now);

            Assert.IsNotNull(match);
            Assert.AreEqual("Whitewitch", match.PlayerName);
            Assert.AreEqual(match.MaxRoll, 333);
            Assert.AreEqual(match.Roll, 195);
        }

        [TestMethod]
        public void RandomParseTestDelay()
        {
            var service = container.Resolve<RandomParser>();
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.", DateTime.Now);
            Assert.IsNull(match);
            System.Threading.Thread.Sleep(3000);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.", DateTime.Now);
            Assert.IsNull(match);
        }

        [TestMethod]
        public void RandomParseTestDuplicates()
        {
            var service = container.Resolve<RandomParser>();
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.", DateTime.Now);
            Assert.IsNull(match);
            match = service.Parse("**A Magic Die is rolled by Steve.", DateTime.Now);
            Assert.IsNull(match);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.", DateTime.Now);
            Assert.IsNotNull(match);
            Assert.AreEqual("Steve", match.PlayerName);
            Assert.AreEqual(match.MaxRoll, 333);
            Assert.AreEqual(match.Roll, 195);
        }
    }
}
