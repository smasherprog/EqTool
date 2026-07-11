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

        // A leftover duplicate from when a built-in shipped as a plain user trigger (no BuiltInId)
        // is merged into the built-in by name: the library supplies the general section, the user's
        // copy supplies its settings/enabled state, and the redundant seeded entry is dropped.
        [TestMethod]
        public void OrphanedDuplicateMergesIntoBuiltInByName()
        {
            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            var seeded = settings.Triggers.First(x => x.BuiltInId == "builtin:cant-see-target");
            seeded.TriggerEnabled = false;
            var orphanId = System.Guid.NewGuid();
            settings.Triggers.Add(new Trigger
            {
                TriggerId = orphanId,
                TriggerName = "Can't See Target",
                SearchText = "old user pattern",
                TriggerEnabled = true,
                DisplayTextEnabled = true,
                DisplayText = "user display text"
            });

            var changed = EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            Assert.IsTrue(changed, "Adopting a duplicate should be reported so it gets persisted.");
            var matches = settings.Triggers.Where(x => x.TriggerName == "Can't See Target").ToList();
            Assert.AreEqual(1, matches.Count, "The duplicate and the seeded built-in should merge into one trigger.");
            var merged = matches[0];
            Assert.AreEqual("builtin:cant-see-target", merged.BuiltInId);
            Assert.IsTrue(merged.IsBuiltIn);
            Assert.AreEqual(orphanId, merged.TriggerId, "The user copy's id should be kept.");
            Assert.AreEqual("^You can't see your target", merged.SearchText, "The general section should come from the built-in library.");
            Assert.AreEqual("user display text", merged.DisplayText, "The user's output settings should be carried over.");
            Assert.IsTrue(merged.TriggerEnabled, "The trigger stays enabled when either copy was enabled.");
            Assert.IsTrue(merged.Customized, "The merge must be marked Customized so later syncs keep the carried-over settings.");
        }

        // The same merge also matches on search text when the names differ, and the general
        // section (including the name) still comes from the library definition.
        [TestMethod]
        public void OrphanedDuplicateMergesIntoBuiltInBySearchText()
        {
            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            settings.Triggers.Add(new Trigger
            {
                TriggerName = "my fizzle alert",
                SearchText = "^Your spell fizzles!",
                TriggerEnabled = true,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "user fizzle text",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "user fizzle tts"
                }
            });

            EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            Assert.IsFalse(settings.Triggers.Any(x => x.TriggerName == "my fizzle alert"), "The duplicate should have been absorbed.");
            var merged = settings.Triggers.Single(x => x.BuiltInId == "builtin:spell-fizzle");
            Assert.AreEqual("Spell Fizzle", merged.TriggerName, "The name should come from the built-in library.");
            Assert.AreEqual("user fizzle text", merged.Basic.DisplayText, "The user's output settings should be carried over.");
            Assert.IsTrue(merged.Customized);
        }

        // Several encounter AOE built-ins share one search pattern (differing only by zone), so a
        // search-text match alone is ambiguous there and must not adopt the user trigger.
        [TestMethod]
        public void AmbiguousSearchTextDuplicateIsLeftAlone()
        {
            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            var before = settings.Triggers.Count;
            settings.Triggers.Add(new Trigger
            {
                TriggerName = "my silver breath",
                SearchText = @"(You feel your skin freeze\.|skin freezes\.|You resist the Silver Breath spell!)",
                TriggerEnabled = true
            });

            EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            var user = settings.Triggers.SingleOrDefault(x => x.TriggerName == "my silver breath");
            Assert.IsNotNull(user, "An ambiguous duplicate should stay a user trigger.");
            Assert.IsNull(user.BuiltInId);
            Assert.AreEqual(before + 1, settings.Triggers.Count);
        }

        // If the user already customized the built-in itself, a same-named user trigger is left
        // alone rather than guessing which copy's settings should win.
        [TestMethod]
        public void DuplicateOfACustomizedBuiltInIsLeftAlone()
        {
            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            var seeded = settings.Triggers.First(x => x.BuiltInId == "builtin:cant-see-target");
            seeded.Customized = true;
            seeded.SearchText = "edited by user";
            settings.Triggers.Add(new Trigger
            {
                TriggerName = "Can't See Target",
                SearchText = "old user pattern",
                TriggerEnabled = true
            });

            EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            Assert.AreEqual(2, settings.Triggers.Count(x => x.TriggerName == "Can't See Target"), "Neither copy should be merged or dropped.");
            Assert.AreEqual("edited by user", settings.Triggers.Single(x => x.BuiltInId == "builtin:cant-see-target").SearchText);
        }

        // A copy of a built-in the user filed into their own folder (the Copy feature clears
        // BuiltInId) is intentional and must not be absorbed back into the library.
        [TestMethod]
        public void BuiltInCopyInAFolderIsNotAdopted()
        {
            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            var folderId = System.Guid.NewGuid();
            settings.TriggerFolders.Add(new TriggerFolder { Id = folderId, Name = "My Folder" });
            settings.Triggers.Add(new Trigger
            {
                TriggerName = "Can't See Target",
                SearchText = "^You can't see your target",
                FolderId = folderId,
                TriggerEnabled = true
            });

            EQToolSettingsLoad.SyncBuiltInTriggers(settings);

            var copy = settings.Triggers.SingleOrDefault(x => x.TriggerName == "Can't See Target" && string.IsNullOrEmpty(x.BuiltInId));
            Assert.IsNotNull(copy, "The filed copy should remain an independent user trigger.");
            Assert.AreEqual(folderId, copy.FolderId);
        }

        // Tells You must fire for real players (single-word names) but not for NPCs, whose names
        // contain spaces (merchants, bankers, quest NPCs), and not for the filtered NPC/pet texts.
        [TestMethod]
        public void TellsYouIgnoresNpcSendersWithSpacesInTheirName()
        {
            var trigger = BuiltInTriggers.CreateTellsYou();
            trigger.TriggerEnabled = true;
            trigger.PlayerName = "Gandalf";

            Assert.IsTrue(trigger.Matches("Thalistair tells you, 'omw'"), "A player tell should fire.");
            Assert.IsTrue(trigger.Matches("Thalistair -> Gandalf: omw"), "The tell-window format should fire.");
            Assert.AreEqual("Thalistair sent a tell", trigger.Expand(trigger.GetEffectiveBasic().DisplayText));

            Assert.IsFalse(trigger.Matches("Peron ThreadSpinner tells you, 'That'll be 3 gold 2 copper for the Earring of the Frozen Skull.'"), "A merchant (multi-word name) should not fire.");
            Assert.IsFalse(trigger.Matches("Cleonae Kalen tells you, 'I'll give you 9 gold 8 silver 8 copper per Globe of Fear'"), "A merchant buy offer should not fire.");
            Assert.IsFalse(trigger.Matches("a spectre tells you, 'Attacking a spectre Master.'"), "A pet attack message should not fire.");
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
