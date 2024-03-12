using Autofac;
using EQToolShared.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class ZoneSpawnTimeTests
    {
        private readonly IContainer container;
        public ZoneSpawnTimeTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void TestPQSpawnTimes()
        {
            ZoneSpawnTimes.isProjectQ = true;
            var spawntime = ZoneSpawnTimes.GetSpawnTime("A Death Beetle", "unrest");
            Assert.AreEqual(500, spawntime.TotalSeconds);

            spawntime = ZoneSpawnTimes.GetSpawnTime("A bok ghoul knight", "lguk");
            Assert.AreEqual(500, spawntime.TotalSeconds);
        }
    }
}
