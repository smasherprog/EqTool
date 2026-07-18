using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class InstallPathCheckerTests
    {
        [TestMethod]
        [DataRow("C:\\Program Files\\EverQuest", true)]
        [DataRow("C:\\Program Files (x86)\\Sony\\EverQuest", true)]
        [DataRow("D:\\Games\\EverQuest", false)]
        [DataRow("C:\\EverQuest", false)]
        public void DetectsProgramFiles(string path, bool expected)
        {
            Assert.AreEqual(expected, InstallPathChecker.IsUnderProgramFiles(path));
        }

        [TestMethod]
        [DataRow("C:\\EQ\\Pigparse", "C:\\EQ", true)]
        [DataRow("C:\\EQ", "C:\\EQ", true)]
        [DataRow("C:\\EQ\\", "C:\\EQ", true)]
        [DataRow("C:\\EQ/Pigparse", "C:\\EQ", true)]
        [DataRow("C:\\Other\\Pigparse", "C:\\EQ", false)]
        [DataRow("C:\\EQ2\\Pigparse", "C:\\EQ", false)]
        public void DetectsInsideDirectory(string path, string root, bool expected)
        {
            Assert.AreEqual(expected, InstallPathChecker.IsInside(path, root));
        }

        [TestMethod]
        public void ProgramFilesEqPathProducesWarning()
        {
            var warning = InstallPathChecker.GetEqPathWarning("C:\\Program Files\\EverQuest");
            Assert.IsFalse(string.IsNullOrEmpty(warning));
            StringAssert.Contains(warning, "Program Files");
        }

        [TestMethod]
        public void CleanEqPathHasNoWarning()
        {
            Assert.AreEqual(string.Empty, InstallPathChecker.GetEqPathWarning("C:\\Games\\EverQuest"));
        }

        [TestMethod]
        public void EmptyEqPathHasNoWarning()
        {
            Assert.AreEqual(string.Empty, InstallPathChecker.GetEqPathWarning(""));
        }
    }
}
