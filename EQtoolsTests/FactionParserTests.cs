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
                Assert.AreEqual("ClawsofVeeshan", e.Faction);
                Assert.AreEqual(EQTool.Models.FactionStatus.GotBetter, e.FactionStatus);
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
                Assert.AreEqual("Coldain", e.Faction);
                Assert.AreEqual(EQTool.Models.FactionStatus.CouldNotGetBetter, e.FactionStatus);
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
                Assert.AreEqual("Coldain", e.Faction);
                Assert.AreEqual(EQTool.Models.FactionStatus.CouldNotGetWorse, e.FactionStatus);
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
                Assert.AreEqual("ClawsofVeeshan", e.Faction);
                Assert.AreEqual(EQTool.Models.FactionStatus.GotWorse, e.FactionStatus);
                IsCalled = true;
            };
            _ = factionParser.Handle("Your faction standing with ClawsofVeeshan got worse.", DateTime.Now, 0);
            Assert.IsTrue(IsCalled);
        }
    }
}
