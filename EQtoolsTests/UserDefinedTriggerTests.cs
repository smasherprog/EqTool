using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQtoolsTests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class UserDefinedTriggerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly TextToSpeachFake textToSpeach;

        public UserDefinedTriggerTests()
        {
            logParser = container.Resolve<LogParser>();
            textToSpeach = container.Resolve<ITextToSpeach>() as TextToSpeachFake;
        }

        [TestMethod]
        public void ZeroNamedGroups()
        {
            // add in a testing trigger to the list of user defined triggers
            var settings = this.container.Resolve<EQToolSettings>();
            settings.Triggers.Add(new Trigger { TriggerEnabled = true, TriggerName = "Test Trigger1", SearchText = "^Can you hear me now?", DisplayTextEnabled = true, DisplayText = "I can hear you", AudioEnabled = true, AudioText = "I can hear you" });

            var called = false;
            textToSpeach.TextToSpeachCallback = (text) =>
            {
                Assert.AreEqual("I can hear you", text);
                called = true;
            };

            logParser.Push("Can you hear me now?", DateTime.Now);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ThreeNamedGroups()
        {
            // add in a testing trigger to the list of user defined triggers
            var settings = this.container.Resolve<EQToolSettings>();
            settings.Triggers.Add(new Trigger { TriggerEnabled = true, TriggerName = "Test Trigger2", SearchText = "^{count} {containers} of {beverage} on the wall", DisplayTextEnabled = true, DisplayText = "{count} {containers} of {beverage}", AudioEnabled = true, AudioText = "{count} {containers} of {beverage}" });

            var called = false;
            textToSpeach.TextToSpeachCallback = (text) =>
            {
                Assert.AreEqual("99 flagons of wine", text);
                called = true;
            };

            logParser.Push("99 flagons of wine on the wall", DateTime.Now);
            Assert.IsTrue(called);
        }




    }
}
