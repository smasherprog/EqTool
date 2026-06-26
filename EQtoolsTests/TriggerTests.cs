using EQTool.Models;
using EQTool.Services;
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

            var line = "99 flagons of wine on the wall";
            var regex = trigger.TriggerRegex;
            var match = regex.Match(line);
            trigger.SaveNamedGroupValues(match);

            Assert.IsTrue(match.Success);
            var audiotext = trigger.ExpandedAudioText;
            var displaytext = trigger.ExpandedDisplayText;

            Assert.AreEqual("99 flagons of wine", audiotext);
            Assert.AreEqual("99 flagons of wine", displaytext);
        }

        [TestMethod]
        public void TestLoadTriggers()
        {
            var t = EQToolSettingsLoad.ReadTriggers();
            Assert.IsNotEmpty(t);
        }

        [TestMethod]
        public void CurrentContextTokenSubstitutesIntoPatternAndOutput()
        {
            var trigger = new Trigger
            {
                SearchText = "{c} has been slain by {s}",
                DisplayTextEnabled = true,
                DisplayText = "{c} died to {s}!",
                TriggerEnabled = true,
                PlayerName = "Gandalf"
            };
            Assert.IsTrue(trigger.Matches("Gandalf has been slain by a Balrog"));
            Assert.AreEqual("Gandalf died to a Balrog!", trigger.ExpandedDisplayText);

        }

        [TestMethod]
        public void CurrentContextTokenRecompilesWhenContextChanges()
        {
            var trigger = new Trigger
            {
                SearchText = "{c} waves",
                TriggerEnabled = true,
                PlayerName = "Gandalf"
            };
            Assert.IsTrue(trigger.Matches("Gandalf waves"));
            Assert.IsFalse(trigger.Matches("Frodo waves"));

            trigger.PlayerName = "Frodo";
            Assert.IsTrue(trigger.Matches("Frodo waves"));
            Assert.IsFalse(trigger.Matches("Gandalf waves"));

        }

        [TestMethod]
        public void CurrentContextTokenEscapesRegexMetacharacters()
        {
            var trigger = new Trigger
            {
                SearchText = "{c} waves",
                TriggerEnabled = true,
                PlayerName = "a.b(c)"
            };
            Assert.IsTrue(trigger.Matches("a.b(c) waves"));
            // the '.' must be literal, not a regex wildcard
            Assert.IsFalse(trigger.Matches("axb(c) waves"));

        }
    }
}
