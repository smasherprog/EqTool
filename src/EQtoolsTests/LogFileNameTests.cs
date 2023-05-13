using EQTool.Models;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class LogFileNameTests
    {
        public LogFileNameTests()
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            var playerinfo = ActivePlayer.GetInfoFromString("eqlog_Vasanle_P1999Green.txt");
            Assert.AreEqual("Vasanle", playerinfo.Name);
            Assert.AreEqual(Servers.Green, playerinfo.Server);
            playerinfo = ActivePlayer.GetInfoFromString("eqlog_Vasanle_P1999Red.txt");
            Assert.AreEqual("Vasanle", playerinfo.Name);
            Assert.AreEqual(Servers.Red, playerinfo.Server);
            playerinfo = ActivePlayer.GetInfoFromString("eqlog_Vasanle_P1999Blue.txt");
            Assert.AreEqual("Vasanle", playerinfo.Name);
            Assert.AreEqual(Servers.Blue, playerinfo.Server);
        }
    }
}
