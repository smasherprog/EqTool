using Autofac;
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
            container = EQToolTests.DI.Init();
        }

        [TestMethod]
        public void Test1()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Dread says 'TINIALITA'");
            Assert.AreEqual("Dread", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }

        [TestMethod]
        public void Test2()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Dread says 'You will not evade me Silvose!'");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Test3()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Fright says 'TINIALITA'");
            Assert.AreEqual("Fright", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }
    }
}
