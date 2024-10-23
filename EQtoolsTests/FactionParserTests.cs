using Autofac;
using EQTool.Services;
using EQTool.Services.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class FactionParserTests : BaseTestClass
    {
        private readonly FactionParser factionParser;
        private readonly LogEvents logEvents;
        private bool IsCalled = false;

        public FactionParserTests()
        {
            factionParser = container.Resolve<FactionParser>();
            logEvents = container.Resolve<LogEvents>();
        }

        [TestMethod]
        public void Parse1()
        {
            logEvents.FactionEvent += (s, e) =>
            {
                Assert.AreEqual(e.Faction, "ClawsofVeeshan");
                Assert.AreEqual(e.FactionStatus, EQTool.Models.FactionStatus.GotBetter);
                IsCalled = true;
            };
            _ = factionParser.Handle("Your faction standing with ClawsofVeeshan got better.", DateTime.Now, 0);
            Assert.IsTrue(IsCalled);
        }

        [TestMethod]
        public void Parse2()
        {
            logEvents.FactionEvent += (s, e) =>
            {
                Assert.AreEqual(e.Faction, "Coldain");
                Assert.AreEqual(e.FactionStatus, EQTool.Models.FactionStatus.CouldNotGetBetter);
                IsCalled = true;
            };
            _ = factionParser.Handle("Your faction standing with Coldain could not possibly get any better.", DateTime.Now, 0);
            Assert.IsTrue(IsCalled);
        }

        [TestMethod]
        public void Parse3()
        {
            logEvents.FactionEvent += (s, e) =>
            {
                Assert.AreEqual(e.Faction, "Coldain");
                Assert.AreEqual(e.FactionStatus, EQTool.Models.FactionStatus.CouldNotGetWorse);
                IsCalled = true;
            };
            _ = factionParser.Handle("Your faction standing with Coldain could not possibly get any worse.", DateTime.Now, 0);
            Assert.IsTrue(IsCalled);
        }

        [TestMethod]
        public void Parse4()
        {
            logEvents.FactionEvent += (s, e) =>
            {
                Assert.AreEqual(e.Faction, "ClawsofVeeshan");
                Assert.AreEqual(e.FactionStatus, EQTool.Models.FactionStatus.GotWorse);
                IsCalled = true;
            };
            _ = factionParser.Handle("Your faction standing with ClawsofVeeshan got worse.", DateTime.Now, 0);
            Assert.IsTrue(IsCalled);
        }
    }
}
