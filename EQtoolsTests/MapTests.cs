using EQToolShared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void HateZoneTest()
        {
            var zone = Zones.Match("You have entered Plane of Hate.");
            Assert.IsNotNull(zone);

            Assert.AreEqual("plane of hate", zone);
            var pretyname = Zones.TranslateToMapName(zone);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void KaelZoneTest()
        {
            var zone = Zones.Match("There is 1 player in Kael Drakkal.");
            Assert.IsNotNull(zone);

            Assert.AreEqual("kael drakkal", zone);
            var pretyname = Zones.TranslateToMapName(zone);
            Assert.AreEqual("kael", pretyname);
        }

        [TestMethod]
        public void HateWhoTest()
        {
            var zone = Zones.Match("There are 15 players in The Plane of Hate.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("the plane of hate", zone);
            var pretyname = Zones.TranslateToMapName(zone);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void POMZoneTest()
        {
            var zone = Zones.Match("You have entered Plane of Mischief.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone);
            var pretyname = Zones.TranslateToMapName(zone);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void POMWhoZoneTest()
        {
            var zone = Zones.Match("There are 4 players in Plane of Mischief.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone);
            var pretyname = Zones.TranslateToMapName(zone);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void NoMatchZones()
        {
            var zone = Zones.Match("There are no players in EverQuest that match those who filters.");
            Assert.AreEqual(string.Empty, zone);
        }

        [TestMethod]
        public void MatchZoneWhoFilter()
        {
            var zone = Zones.Match("There are no players in East Commonlands that match those who filters.");
            Assert.AreEqual(string.Empty, zone);
        }
    }
}
