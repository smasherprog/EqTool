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
                "You are drowning",
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
                "Healed you",
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
        // built-in customization, leaving exactly the fresh-user default set (only built-ins in the
        // top-level Encounters folder are enabled; everything else is seeded disabled).
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
            Assert.IsTrue(settings.Triggers.Where(x => x.BuiltInFolder == "Encounters").All(x => x.TriggerEnabled), "Reset built-ins in the top-level Encounters folder should be enabled by default.");
            Assert.IsTrue(settings.Triggers.Where(x => x.BuiltInFolder != "Encounters").All(x => !x.TriggerEnabled), "Reset built-ins outside the top-level Encounters folder should be disabled by default.");
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

        // "You are drowning" alerts with both display text and TTS audio.
        [TestMethod]
        public void DrowningAlertsWithTextAndAudio()
        {
            var trigger = BuiltInTriggers.CreateDrowning();
            trigger.TriggerEnabled = true;
            trigger.PlayerName = "Gandalf";

            Assert.IsTrue(trigger.Matches("YOU are drowning!"));
            var output = trigger.GetEffectiveBasic();
            Assert.IsTrue(output.DisplayTextEnabled);
            Assert.AreEqual("You are drowning!", trigger.Expand(output.DisplayText));
            Assert.AreEqual(TriggerAudioType.TextToSpeech, output.AudioType);
            Assert.AreEqual("You are drowning", trigger.Expand(output.TtsText));
        }

        // "Healed you" shows who healed you and for how much, display text only (no audio).
        [TestMethod]
        public void HealedYouShowsHealerAndAmountWithoutAudio()
        {
            var trigger = BuiltInTriggers.All().Single(x => x.BuiltInId == "builtin:healed-you");
            trigger.TriggerEnabled = true;
            trigger.PlayerName = "Gandalf";

            Assert.IsTrue(trigger.Matches("Nimsake has healed you for 852 points of damage."));
            Assert.AreEqual("Nimsake healed you for 852.", trigger.Expand(trigger.GetEffectiveBasic().DisplayText));
            Assert.AreEqual(TriggerAudioType.None, trigger.GetEffectiveBasic().AudioType);

            Assert.IsFalse(trigger.Matches("You have healed Nimsake for 200 points of damage."), "Healing someone else should not fire.");
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

        // The FTE %-rule timers (ported from the old FTEHandler) live in the top-level Encounters
        // folder, which is what makes SyncBuiltInTriggers seed them ENABLED for everyone.
        [TestMethod]
        public void FTERuleTriggersAreInTheEncountersFolderSoTheySeedEnabled()
        {
            var all = BuiltInTriggers.All();
            var ids = new[]
            {
                BuiltInTriggers.FTE97RuleBuiltInId,
                BuiltInTriggers.FTE97RuleGreenBuiltInId,
                BuiltInTriggers.FTE96RuleGreenBuiltInId,
                BuiltInTriggers.FTELodizalRuleBuiltInId,
            };
            foreach (var id in ids)
            {
                var trigger = all.FirstOrDefault(x => x.BuiltInId == id);
                Assert.IsNotNull(trigger, $"Built-in '{id}' not found.");
                Assert.AreEqual("Encounters", trigger.BuiltInFolder, $"Built-in '{id}' must be in the top-level Encounters folder so it is enabled by default.");
            }

            var settings = new EQToolSettings();
            EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            foreach (var id in ids)
            {
                Assert.IsTrue(settings.Triggers.Single(x => x.BuiltInId == id).TriggerEnabled, $"Built-in '{id}' should be seeded enabled.");
            }
        }

        // The 97% rule outside Green: 61 second timer for all three rule mobs.
        [TestMethod]
        public void FTE97RuleMatchesRuleMobsAndRunsSixtyOneSeconds()
        {
            var trigger = BuiltInTriggers.CreateFTE97Rule();

            Assert.IsTrue(trigger.Matches("Zlandicar engages Tzvia!"));
            Assert.AreEqual("--97% Rule-- Zlandicar", trigger.Expand(trigger.Timer.TimerName));
            Assert.IsTrue(trigger.Matches("Dozekar the Cursed engages Tzvia!"));
            Assert.AreEqual("--97% Rule-- Dozekar the Cursed", trigger.Expand(trigger.Timer.TimerName));
            Assert.IsTrue(trigger.Matches("Lord Yelinak engages Tzvia!"));

            Assert.IsFalse(trigger.Matches("Cekenar engages Tzvia!"), "A non-rule mob's FTE should not fire.");
            Assert.IsFalse(trigger.Matches("Bob shouts, 'Zlandicar engages Tzvia!'"), "Player chat quoting an FTE should not fire.");

            Assert.AreEqual(61, trigger.Timer.Duration.TotalSeconds);
            Assert.IsFalse(trigger.MatchesServer(EQToolShared.Enums.Servers.Green), "Dozekar/Yelinak use the 96% rule on Green, so the 97% rule trigger must not run there.");
            Assert.IsTrue(trigger.MatchesServer(EQToolShared.Enums.Servers.Blue));
            Assert.IsTrue(trigger.MatchesServer(EQToolShared.Enums.Servers.Red));
            Assert.IsTrue(trigger.MatchesServer(EQToolShared.Enums.Servers.Quarm));
        }

        // On Green, Zlandicar keeps the 97% rule while Dozekar/Yelinak get the 91 second 96% rule.
        [TestMethod]
        public void FTEGreenRulesSplitZlandicarFromDozekarAndYelinak()
        {
            var green97 = BuiltInTriggers.CreateFTE97RuleGreen();
            Assert.IsTrue(green97.Matches("Zlandicar engages Tzvia!"));
            Assert.IsFalse(green97.Matches("Dozekar the Cursed engages Tzvia!"));
            Assert.AreEqual(61, green97.Timer.Duration.TotalSeconds);
            Assert.IsTrue(green97.MatchesServer(EQToolShared.Enums.Servers.Green));
            Assert.IsFalse(green97.MatchesServer(EQToolShared.Enums.Servers.Blue));

            var green96 = BuiltInTriggers.CreateFTE96RuleGreen();
            Assert.IsTrue(green96.Matches("Dozekar the Cursed engages Tzvia!"));
            Assert.AreEqual("--96% Rule-- Dozekar the Cursed", green96.Expand(green96.Timer.TimerName));
            Assert.IsTrue(green96.Matches("Lord Yelinak engages Tzvia!"));
            Assert.IsFalse(green96.Matches("Zlandicar engages Tzvia!"), "Zlandicar stays on the 97% rule on Green.");
            Assert.AreEqual(91, green96.Timer.Duration.TotalSeconds);
            Assert.IsTrue(green96.MatchesServer(EQToolShared.Enums.Servers.Green));
            Assert.IsFalse(green96.MatchesServer(EQToolShared.Enums.Servers.Blue));
        }

        // Lodizal's 5 minute rule runs on every server.
        [TestMethod]
        public void FTELodizalRuleRunsFiveMinutesOnEveryServer()
        {
            var trigger = BuiltInTriggers.CreateFTELodizalRule();

            Assert.IsTrue(trigger.Matches("Lodizal engages Tzvia!"));
            Assert.AreEqual("--5 Minute Rule-- Lodizal", trigger.Expand(trigger.Timer.TimerName));
            Assert.AreEqual(5, trigger.Timer.Duration.TotalMinutes);
            Assert.IsTrue(trigger.MatchesServer(EQToolShared.Enums.Servers.Green));
            Assert.IsTrue(trigger.MatchesServer(EQToolShared.Enums.Servers.Blue));
            Assert.IsTrue(trigger.MatchesServer(null), "An unrestricted trigger fires even when the server is unknown.");
        }

        // A server-restricted trigger must not fire when the player's server is unknown.
        [TestMethod]
        public void ServerRestrictedTriggerDoesNotFireOnUnknownServer()
        {
            Assert.IsFalse(BuiltInTriggers.CreateFTE96RuleGreen().MatchesServer(null));
            Assert.IsFalse(BuiltInTriggers.CreateFTE97Rule().MatchesServer(null));
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

        // "{3}" in a pattern is a regex quantifier, not a simplified {name} placeholder. Converting
        // it into "(?<3>[\w` ]+)" silently changed what the pattern matched.
        [TestMethod]
        public void RegexQuantifiersAreNotTreatedAsPlaceholders()
        {
            var trigger = new Trigger
            {
                SearchText = @"^You have gained \d{3} experience",
                TriggerEnabled = true
            };

            Assert.IsTrue(trigger.Matches("You have gained 120 experience"));
            Assert.IsFalse(trigger.Matches("You have gained 12 experience"), "The {3} quantifier must still mean 'exactly three digits'.");
            Assert.IsFalse(trigger.Matches("You have gained many experience"));
        }

        // "{0}" used to convert to "(?<0>...)", which throws ("capture number cannot be zero") and
        // left the trigger permanently unable to compile - it silently never fired again.
        [TestMethod]
        public void ZeroQuantifierDoesNotKillTheTrigger()
        {
            var trigger = new Trigger
            {
                SearchText = @"^Nothing\d{0}here",
                TriggerEnabled = true
            };

            Assert.IsNotNull(trigger.TriggerRegex, "The pattern must still compile.");
            Assert.IsTrue(trigger.Matches("Nothinghere"));
        }

        // A placeholder with no captured value (a typo, or a group that didn't participate) must not
        // shift the values that follow it. This used to produce "1000 {damage}".
        [TestMethod]
        public void UnknownPlaceholderDoesNotShiftLaterValues()
        {
            var trigger = new Trigger
            {
                SearchText = "^{attacker} hits you for {damage} points",
                DisplayTextEnabled = true,
                DisplayText = "{typo} took {damage} from {attacker}",
                TriggerEnabled = true
            };

            Assert.IsTrue(trigger.Matches("a Balrog hits you for 1000 points"));
            Assert.AreEqual("{typo} took 1000 from a Balrog", trigger.ExpandedDisplayText);
        }

        // Captured values are inserted literally: a '$' in a value must not be read as a
        // substitution pattern ($1, $&, ...) by the placeholder replacement.
        [TestMethod]
        public void CapturedValueContainingDollarSignIsInsertedLiterally()
        {
            var trigger = new Trigger
            {
                SearchText = @"^Sold for (?<price>[$\w]+) today",
                DisplayTextEnabled = true,
                DisplayText = "price was {price}",
                TriggerEnabled = true
            };

            Assert.IsTrue(trigger.Matches("Sold for $5 today"));
            Assert.AreEqual("price was $5", trigger.ExpandedDisplayText);
        }

        // A pattern that backtracks pathologically must give up instead of hanging the UI thread
        // forever, and must not pay the full timeout again on every later line.
        [TestMethod]
        public void CatastrophicBacktrackingTimesOutInsteadOfHanging()
        {
            var trigger = new Trigger
            {
                // nested quantifier: the engine can split the run of 'a's between the inner and
                // outer '+' in exponentially many ways, and must try all of them before failing
                SearchText = "^(a+)+$",
                TriggerEnabled = true
            };
            // never matches (the trailing '!' can't be consumed), so every split gets explored
            var line = new string('a', 32) + "!";

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Assert.IsFalse(trigger.Matches(line));
            watch.Stop();
            Assert.IsLessThan(5000, watch.ElapsedMilliseconds, "The match must time out rather than run unbounded.");
            // guards against this test going vacuous if the pattern above stops being pathological:
            // an unbounded run of this pattern takes many minutes, so a fast return here would mean
            // the timeout path was never exercised
            Assert.IsGreaterThan(100, watch.ElapsedMilliseconds, "The pattern must actually have hit the match timeout.");

            // the pattern is parked after timing out, so later lines are rejected immediately
            watch.Restart();
            Assert.IsFalse(trigger.Matches(line));
            watch.Stop();
            Assert.IsLessThan(50, watch.ElapsedMilliseconds, "A timed-out pattern must not be retried at full cost on every line.");
        }
    }
}
