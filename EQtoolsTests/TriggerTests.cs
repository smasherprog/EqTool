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
            Assert.IsTrue(match);

            Assert.AreEqual(trigger.DisplayText, "99 flagons of wine");
            Assert.AreEqual(trigger.AudioText, "99 flagons of wine");
        }
    }
}
