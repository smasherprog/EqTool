using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class FindEqTests
    {
        [TestMethod]
        public void HateZoneTest()
        {
            var eqfind = new FindEq();
            var eqpath = eqfind.LoadEQPath();
            _ = FindEq.HasLogFiles(eqpath.EQlogLocation);
        }
    }
}
