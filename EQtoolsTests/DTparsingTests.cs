using Autofac;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class DTparsingTests : BaseTestClass
    { 
        public DTparsingTests()
        { 
        }

        [TestMethod]
        public void Test1()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Dread says 'TINIALITA'", DateTime.Now);
            Assert.AreEqual("Dread", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }

        [TestMethod]
        public void Test2()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Dread says 'You will not evade me Silvose!'", DateTime.Now);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Test3()
        {
            var result = container.Resolve<DeathTouchParser>().DtCheck("Fright says 'TINIALITA'", DateTime.Now);
            Assert.AreEqual("Fright", result.NpcName);
            Assert.AreEqual("TINIALITA", result.DTReceiver);
        }
    }
}
