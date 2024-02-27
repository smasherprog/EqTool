using Autofac;
using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.");
            Assert.IsNull(match);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.");

            Assert.IsNotNull(match);
            Assert.AreEqual("Whitewitch", match.PlayerName);
            Assert.AreEqual(match.MaxRoll, 333);
            Assert.AreEqual(match.Roll, 195);
        }

        [TestMethod]
        public void RandomParseTestDelay()
        {
            var service = container.Resolve<RandomParser>();
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.");
            Assert.IsNull(match);
            System.Threading.Thread.Sleep(3000);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.");
            Assert.IsNull(match);
        }

        [TestMethod]
        public void RandomParseTestDuplicates()
        {
            var service = container.Resolve<RandomParser>();
            var match = service.Parse("**A Magic Die is rolled by Whitewitch.");
            Assert.IsNull(match);
            match = service.Parse("**A Magic Die is rolled by Steve.");
            Assert.IsNull(match);
            match = service.Parse("**It could have been any number from 0 to 333, but this time it turned up a 195.");
            Assert.IsNotNull(match);
            Assert.AreEqual("Steve", match.PlayerName);
            Assert.AreEqual(match.MaxRoll, 333);
            Assert.AreEqual(match.Roll, 195);
        }
    }
}
