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

            string line = "99 flagons of wine on the wall";
            var regex = trigger.TriggerRegex;
            var match = regex.Match(line);
            trigger.SaveNamedGroupValues(match);

            Assert.IsTrue(match.Success);
            var audiotext = trigger.ConvertedAudioText;
            var displaytext = trigger.ConvertedDisplayText;

            Assert.AreEqual(audiotext, "99 flagons of wine");
            Assert.AreEqual(displaytext, "99 flagons of wine");
        }
    }
}
