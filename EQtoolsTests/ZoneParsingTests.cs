using Autofac;
using EQTool.Services.Parsing;
using EQToolShared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class ZoneParsingTests : BaseTestClass
    {
        private readonly YouZonedParser youZonedParser;

        public ZoneParsingTests()
        {
            youZonedParser = container.Resolve<YouZonedParser>();
        }

        [TestMethod]
        public void HateZoneTest()
        {
            var zone = youZonedParser.ZoneChanged("You have entered Plane of Hate.", DateTime.Now, 0);
            Assert.IsNotNull(zone);

            Assert.AreEqual("plane of hate", zone.LongName);
            var pretyname = Zones.TranslateToMapName(zone.LongName);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void KaelZoneTest()
        {
            var zone = youZonedParser.ZoneChanged("There is 1 player in Kael Drakkal.", DateTime.Now, 0);
            Assert.IsNotNull(zone);

            Assert.AreEqual("kael drakkal", zone.LongName);
            var pretyname = Zones.TranslateToMapName(zone.LongName);
            Assert.AreEqual("kael", pretyname);
        }

        [TestMethod]
        public void HateWhoTest()
        {
            var zone = youZonedParser.ZoneChanged("There are 15 players in The Plane of Hate.", DateTime.Now, 0);
            Assert.IsNotNull(zone);
            Assert.AreEqual("the plane of hate", zone.LongName);
            var pretyname = Zones.TranslateToMapName(zone.LongName);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void POMZoneTest()
        {
            var zone = youZonedParser.ZoneChanged("You have entered Plane of Mischief.", DateTime.Now, 0);
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone.LongName);
            var pretyname = Zones.TranslateToMapName(zone.LongName);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void POMWhoZoneTest()
        {
            var zone = youZonedParser.ZoneChanged("There are 4 players in Plane of Mischief.", DateTime.Now, 0);
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone.LongName);
            var pretyname = Zones.TranslateToMapName(zone.LongName);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void NoMatchZones()
        {
            var zone = youZonedParser.ZoneChanged("There are no players in EverQuest that match those who filters.", DateTime.Now, 0);
            Assert.IsNull(zone);
        }

        [TestMethod]
        public void MatchZoneWhoFilter()
        {
            var zone = youZonedParser.ZoneChanged("There are no players in East Commonlands that match those who filters.", DateTime.Now, 0);
            Assert.IsNull(zone);
        }
    }
}
