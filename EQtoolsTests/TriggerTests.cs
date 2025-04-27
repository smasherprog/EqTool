using EQTool.Models;
using EQTool.Services.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class TriggerTests
    {
        public TriggerTests()
        {
        }

        [TestMethod]
        public void HappyPathTest()
        {
            var trigger = new Trigger
            {
                SearchText = "^{count} {containers} of {beverage} on the wall",
                DisplayTextEnabled = true,
                DisplayText = "{count} {containers} of {beverage}",
                AudioTextEnabled = true,
                AudioText = "{count} {containers} of {beverage}",
                TriggerEnabled = true
            };
            var match = TriggerHandler.Match(trigger, "99 flagons of wine on the wall");
            Assert.IsTrue(match.match);
            var audiotext = TriggerHandler.ProcessOutputText(trigger.AudioText, match.nameValuePairs);
            var displaytext = TriggerHandler.ProcessOutputText(trigger.DisplayText, match.nameValuePairs);

            Assert.AreEqual(audiotext, "99 flagons of wine");
            Assert.AreEqual(displaytext, "99 flagons of wine");
        }
    }
}
