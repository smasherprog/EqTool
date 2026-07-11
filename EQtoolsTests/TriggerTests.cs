using EQTool.Models;
using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
            var t = BuiltInTriggers.All();
            Assert.IsNotEmpty(t);
        }

        // The general-purpose (non-encounter) built-ins live in the Utility folder so the
        // Built In tree isn't a flat pile of loose triggers.
        [TestMethod]
        public void UtilityBuiltInsAreInTheUtilityFolder()
        {
            var expected = new[]
            {
                "Corpse Need Consent",
                "Corpse Out of Range",
                "Entered Zone",
                "Exp Timer",
                "Group Invite",
                "Invis Fading",
                "Levitate Fading",
                "Regen Faded",
                "Select a Target",
                "Sense Heading",
                "Sense Heading Failed",
                "Tells You",
            };
            var all = BuiltInTriggers.All();
            foreach (var name in expected)
            {
                var trigger = all.FirstOrDefault(x => x.TriggerName == name);
                Assert.IsNotNull(trigger, $"Built-in '{name}' not found.");
                Assert.AreEqual("Utility", trigger.BuiltInFolder, $"Built-in '{name}' should be in the Utility folder.");
            }
        }

        // The spell/combat feedback built-ins live in the Combat folder, and nothing is left
        // loose at the Built In root - every built-in belongs to a folder.
        [TestMethod]
        public void CombatBuiltInsAreInTheCombatFolderAndNoneAreLoose()
        {
            var expected = new[]
            {
                "Backstabber",
                "Can't See Target",
                "Charm Break",
                "Dispelled",
                "Failed Feign",
                "Insufficient Mana",
                "Must be standing to cast",
                "NPC Gating",
                "Resist",
                "Spell Did Not Take Hold",
                "Spell Fizzle",
                "Spell Interrupted",
                "Spell Worn Off",
                "Target Out of Range",
            };
            var all = BuiltInTriggers.All();
            foreach (var name in expected)
            {
                var trigger = all.FirstOrDefault(x => x.TriggerName == name);
                Assert.IsNotNull(trigger, $"Built-in '{name}' not found.");
                Assert.AreEqual("Combat", trigger.BuiltInFolder, $"Built-in '{name}' should be in the Combat folder.");
            }

            var loose = all.Where(x => string.IsNullOrWhiteSpace(x.BuiltInFolder)).Select(x => x.TriggerName).ToList();
            Assert.AreEqual(0, loose.Count, "These built-ins have no folder: " + string.Join(", ", loose));
        }

        // Mirrors what SettingsManagementViewModel.ResetTriggersToDefault does: clearing the trigger
        // and folder lists then re-seeding built-ins must drop every user trigger/folder and every
        // built-in customization, leaving exactly the fresh-user default set (all built-ins enabled).
        [TestMethod]
        public void ResettingTriggersRestoresBuiltInDefaults()
        {
            var settings = new EQToolSettings();
            // A brand-new user's seed, then some user changes on top of it.
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            var customizedBuiltIn = settings.Triggers.First(x => !string.IsNullOrEmpty(x.BuiltInId));
            customizedBuiltIn.Customized = true;
            customizedBuiltIn.TriggerEnabled = false;
            customizedBuiltIn.SearchText = "edited by user";
            var userFolderId = System.Guid.NewGuid();
            settings.TriggerFolders.Add(new TriggerFolder { Id = userFolderId, Name = "My Folder" });
            settings.Triggers.Add(new Trigger { TriggerName = "My Trigger", SearchText = "mine", FolderId = userFolderId });

            // The reset.
            settings.Triggers = new System.Collections.Generic.List<Trigger>();
            settings.TriggerFolders = new System.Collections.Generic.List<TriggerFolder>();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            var expected = BuiltInTriggers.All()
                .Where(x => !string.IsNullOrEmpty(x.BuiltInId))
                .Select(x => x.BuiltInId)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .Count();
            Assert.AreEqual(0, settings.TriggerFolders.Count, "User folders should be gone after reset.");
            Assert.IsFalse(settings.Triggers.Any(x => x.TriggerName == "My Trigger"), "User triggers should be gone after reset.");
            Assert.IsTrue(settings.Triggers.All(x => !string.IsNullOrEmpty(x.BuiltInId)), "Only built-in triggers should remain.");
            Assert.AreEqual(expected, settings.Triggers.Count, "All built-in triggers should be restored.");
            Assert.IsTrue(settings.Triggers.All(x => x.TriggerEnabled), "Reset built-ins should be enabled by default.");
            Assert.IsTrue(settings.Triggers.All(x => !x.Customized), "No built-in should remain customized after reset.");
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
        public void CounterTokenReflectsMatchCount()
        {
            var trigger = new Trigger
            {
                SearchText = "You hit the target",
                DisplayTextEnabled = true,
                DisplayText = "Hit number {COUNTER}",
                TriggerEnabled = true,
                PlayerName = "Gandalf"
            };

            Assert.IsTrue(trigger.Matches("You hit the target"));
            _ = trigger.CurrentCounter++;
            Assert.AreEqual("Hit number 1", trigger.ExpandedDisplayText);

            _ = trigger.CurrentCounter++;
            Assert.AreEqual("Hit number 2", trigger.ExpandedDisplayText);
        }

        [TestMethod]
        public void CounterTokenMatchesEveryCase()
        {
            var trigger = new Trigger
            {
                DisplayTextEnabled = true,
                DisplayText = "{COUNTER} {counter} {Counter} {CoUnTeR}",
                PlayerName = "Gandalf"
            };

            trigger.CurrentCounter = 7;
            // every casing of the macro must resolve to the same count
            Assert.AreEqual("7 7 7 7", trigger.ExpandedDisplayText);
        }

        [TestMethod]
        public void CounterTokenIsCaseInsensitiveAndResetsWhenCharacterChanges()
        {
            var trigger = new Trigger
            {
                DisplayTextEnabled = true,
                DisplayText = "count={counter}",
                PlayerName = "Gandalf"
            };

            _ = trigger.CurrentCounter++;
            _ = trigger.CurrentCounter++;
            Assert.AreEqual("count=2", trigger.ExpandedDisplayText);

            // switching characters starts the tally over
            trigger.PlayerName = "Frodo";
            Assert.AreEqual("count=0", trigger.ExpandedDisplayText);
            _ = trigger.CurrentCounter++;
            Assert.AreEqual("count=1", trigger.ExpandedDisplayText);

            // the time-based reset clears it too
            trigger.CurrentCounter = 0;
            Assert.AreEqual("count=0", trigger.ExpandedDisplayText);
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
