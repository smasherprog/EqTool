using EQTool.Services.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public void HateZoneTest()
        {
            var zone = ZoneParser.Match("[Sat Oct 08 11:37:45 2022] You have entered Plane of Hate.");
            Assert.IsNotNull(zone);

            Assert.AreEqual("plane of hate", zone);
            var pretyname = ZoneParser.TranslateToMapName(zone);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void HateWhoTest()
        {
            var zone = ZoneParser.Match("[Sat Oct 08 11:31:38 2022] There are 15 players in The Plane of Hate.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("the plane of hate", zone);
            var pretyname = ZoneParser.TranslateToMapName(zone);
            Assert.AreEqual("hateplane", pretyname);
        }

        [TestMethod]
        public void POMZoneTest()
        {
            var zone = ZoneParser.Match("[Sat Oct 08 13:36:33 2022] You have entered Plane of Mischief.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone);
            var pretyname = ZoneParser.TranslateToMapName(zone);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void POMWhoZoneTest()
        {
            var zone = ZoneParser.Match("[Thu Dec 15 07:04:14 2022] There are 4 players in Plane of Mischief.");
            Assert.IsNotNull(zone);
            Assert.AreEqual("plane of mischief", zone);
            var pretyname = ZoneParser.TranslateToMapName(zone);
            Assert.AreEqual("mischiefplane", pretyname);
        }

        [TestMethod]
        public void NoMatchZones()
        {
            var zone = ZoneParser.Match("[Tue Feb 21 20:36:39 2023] There are no players in EverQuest that match those who filters.");
            Assert.AreEqual(string.Empty, zone);
        }
    }
}
