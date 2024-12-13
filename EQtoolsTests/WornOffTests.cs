using EQTool.Services;
using EQTool.ViewModels.SpellWindow;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EQTool.Services.Parsing;
using EQTool.Models;

namespace EQtoolsTests
{
    [TestClass]
    public class WornOffTests : BaseTestClass
    {
        private readonly WornOffParser parser;

        public WornOffTests()
        {
            parser = container.Resolve<WornOffParser>();
        }

        [TestMethod]
        public void VenomOfTheSnake()
        {
            var now = DateTime.Now;
            var line = "Your Venom of the Snake spell has worn off.";
            var e = parser.WornOffCheck(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Venom of the Snake", e.SpellName);
        }

        [TestMethod]
        public void BoilBlood()
        {
            var now = DateTime.Now;
            var line = "Your Boil Blood spell has worn off.";
            var e = parser.WornOffCheck(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Boil Blood", e.SpellName);
        }

        [TestMethod]
        public void Fear()
        {
            var now = DateTime.Now;
            var line = "Your Fear spell has worn off.";
            var e = parser.WornOffCheck(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Fear", e.SpellName);
        }

    }
}
