using Autofac;
using EQTool.Services;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class DTparsingTests
    {
        private readonly IContainer container;
        public DTparsingTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void Test1()
        {
            var result = new POFDTParser().DtCheck("Dread says 'TINIALITA'");
            Assert.AreEqual("Dread", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }

        [TestMethod]
        public void Test2()
        {
            var result = new POFDTParser().DtCheck("Dread says 'You will not evade me Silvose!'");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Test3()
        {
            var result = new POFDTParser().DtCheck("Fright says 'TINIALITA'");
            Assert.AreEqual("Fright", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }
    }
}
