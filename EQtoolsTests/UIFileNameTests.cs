using EQToolShared;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class UIFileNameTests
    {
        [TestMethod]
        [DataRow("UI_Pigy_P1999Green.ini", "Pigy", Servers.Green, true)]
        [DataRow("Pigy_P1999Green.ini", "Pigy", Servers.Green, false)]
        [DataRow("UI_Bob_P1999Blue.ini", "Bob", Servers.Blue, true)]
        [DataRow("UI_Killer_P1999PVP.ini", "Killer", Servers.Red, true)]
        [DataRow("Killer_P1999PVP.ini", "Killer", Servers.Red, false)]
        public void ParsesPairFiles(string fileName, string expectedName, Servers expectedServer, bool expectedIsUi)
        {
            var ok = UIFileName.TryParse(fileName, out var info);
            Assert.IsTrue(ok);
            Assert.AreEqual(expectedName, info.PlayerName);
            Assert.AreEqual(expectedServer, info.Server);
            Assert.AreEqual(expectedIsUi, info.IsUiLayoutFile);
        }

        [TestMethod]
        public void ParsesFullPath()
        {
            var ok = UIFileName.TryParse("C:\\Everquest\\UI_Pigy_P1999Green.ini", out var info);
            Assert.IsTrue(ok);
            Assert.AreEqual("Pigy", info.PlayerName);
            Assert.AreEqual(Servers.Green, info.Server);
        }

        [TestMethod]
        [DataRow("eqclient.ini")]
        [DataRow("UI_Pigy_Quarm.ini")]
        [DataRow("Pigy_Quarm.ini")]
        [DataRow("spells_us.txt")]
        [DataRow("UI_P1999Green.ini")]
        [DataRow("")]
        [DataRow(null)]
        public void RejectsNonPairFiles(string fileName)
        {
            Assert.IsFalse(UIFileName.TryParse(fileName, out _));
            Assert.IsFalse(UIFileName.IsUiPairFile(fileName));
        }
    }
}
