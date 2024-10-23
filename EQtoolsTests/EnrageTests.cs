using Autofac;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class EnrageTests : BaseTestClass
    {
        public EnrageTests()
        {
        }

        [TestMethod]
        public void Parse1()
        {
            var service = container.Resolve<EnrageParser>();
            var d = service.EnrageCheck("Cekenar has become ENRAGED.", DateTime.Now, 0);
            Assert.AreEqual(d.NpcName, "Cekenar");
        }
    }
}
