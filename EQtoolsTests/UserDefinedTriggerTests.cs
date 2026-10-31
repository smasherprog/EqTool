using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQtoolsTests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
            var settings = this.container.Resolve<EQToolSettings>();
            settings.Triggers.Add(new Trigger { TriggerEnabled = true, TriggerName = "Test Trigger1", SearchText = "^Can you hear me now?", DisplayTextEnabled = true, DisplayText = "I can hear you", AudioTextEnabled = true, AudioText = "I can hear you" });

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
            var settings = this.container.Resolve<EQToolSettings>();
            settings.Triggers.Add(new Trigger { TriggerEnabled = true, TriggerName = "Test Trigger2", SearchText = "^{count} {containers} of {beverage} on the wall", DisplayTextEnabled = true, DisplayText = "{count} {containers} of {beverage}", AudioTextEnabled = true, AudioText = "{count} {containers} of {beverage}" });

            var called = false;
            textToSpeach.TextToSpeachCallback = (text) =>
            {
                Assert.AreEqual("99 flagons of wine", text);
                called = true;
            };

            logParser.Push("99 flagons of wine on the wall", DateTime.Now);
            Assert.IsTrue(called);
        }

        private List<string> CaptureSpeech()
        {
            var spoken = new List<string>();
            textToSpeach.TextToSpeachCallback = (text) => spoken.Add(text);
            return spoken;
        }

        private Trigger AddSpellTrigger(string spellName, SpellTriggerTarget target, string audioText)
        {
            var trigger = new Trigger
            {
                TriggerEnabled = true,
                TriggerName = "Spell Trigger",
                EventType = TriggerEventType.Spell,
                SpellName = spellName,
                SpellTarget = target,
                AudioTextEnabled = audioText != null,
                AudioText = audioText
            };
            container.Resolve<EQToolSettings>().Triggers.Add(trigger);
            return trigger;
        }

        [TestMethod]
        public void SpellTriggerFiresWhenSpellLandsOnYou()
        {
            _ = AddSpellTrigger("Frostreaver's Blessing", SpellTriggerTarget.You, "{spell} on {target}");
            var spoken = CaptureSpeech();

            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);

            CollectionAssert.Contains(spoken, "Frostreaver's Blessing on You");
        }

        [TestMethod]
        public void SpellTriggerFiresWhenSpellLandsOnSomeoneElse()
        {
            _ = AddSpellTrigger("Divine Barrier", SpellTriggerTarget.Others, "{spell} on {target}");
            var spoken = CaptureSpeech();

            logParser.Push("Someone is surrounded by a divine barrier.", DateTime.Now);

            CollectionAssert.Contains(spoken, "Divine Barrier on Someone");
        }

        [TestMethod]
        public void SpellTriggerSkipsLandingsOnTheWrongTarget()
        {
            _ = AddSpellTrigger("Frostreaver's Blessing", SpellTriggerTarget.Others, "landed");
            var spoken = CaptureSpeech();

            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);

            CollectionAssert.DoesNotContain(spoken, "landed");
        }

        // A trigger switched from Log Text to Spell keeps its old search text; it must not match on it.
        [TestMethod]
        public void SpellTriggerIgnoresItsSearchText()
        {
            var trigger = AddSpellTrigger("Frostreaver's Blessing", SpellTriggerTarget.Anyone, "landed");
            trigger.SearchText = "^Can you hear me now?";
            var spoken = CaptureSpeech();

            logParser.Push("Can you hear me now?", DateTime.Now);

            CollectionAssert.DoesNotContain(spoken, "landed");
        }

        [TestMethod]
        public void SpellTriggerTimerEndedFiresWhenTheSpellWearsOff()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 53;
            var trigger = AddSpellTrigger("Envenomed Bolt", SpellTriggerTarget.Others, null);
            trigger.TimerEnded = new TriggerTimerEnded
            {
                Enabled = true,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "{spell} on {target} wore off" }
            };
            var spoken = CaptureSpeech();

            logParser.Push("Someone has been poisoned.", DateTime.Now);
            Assert.IsEmpty(spoken);
            logParser.Push("Your Envenomed Bolt spell has worn off.", DateTime.Now.AddSeconds(40));

            CollectionAssert.Contains(spoken, "Envenomed Bolt on Someone wore off");
        }

        [TestMethod]
        public void SpellTriggerTimerEndingShowsOverlayText()
        {
            player.Player.Level = 60;
            var trigger = AddSpellTrigger("Aegolism", SpellTriggerTarget.You, null);
            trigger.TimerEnding = new TriggerTimerEnding
            {
                Enabled = true,
                Hours = 2,
                Minutes = 29,
                Seconds = 40,
                Output = new TriggerOutput { DisplayTextEnabled = true, DisplayText = "about to end" }
            };
            var overlays = new List<string>();
            container.Resolve<LogEvents>().OverlayEvent += (s, e) => overlays.Add(e.Text);
            var timers = container.Resolve<TriggerTimerManager>();

            // Aegolism lasts 2h30m, so a 2h29m40s threshold is crossed 20 seconds after it lands
            logParser.Push("You are filled with the power of Aegolism.", DateTime.Now);
            timers.Tick(DateTime.UtcNow.AddSeconds(10));
            CollectionAssert.DoesNotContain(overlays, "about to end");
            timers.Tick(DateTime.UtcNow.AddSeconds(21));

            CollectionAssert.Contains(overlays, "about to end");
        }

        private Trigger AddAegolismTimerTrigger(TriggerTimerEnding ending, TriggerTimerEnded ended)
        {
            player.Player.Level = 60;
            var trigger = AddSpellTrigger("Aegolism", SpellTriggerTarget.You, null);
            trigger.TimerEnding = ending ?? new TriggerTimerEnding();
            trigger.TimerEnded = ended ?? new TriggerTimerEnded();
            return trigger;
        }

        // Aegolism lasts 2h30m; a warning set further out than that would be "reached" on landing.
        [TestMethod]
        public void SpellTriggerTimerEndingSkipsAThresholdTheSpellNeverCountsDownTo()
        {
            _ = AddAegolismTimerTrigger(new TriggerTimerEnding
            {
                Enabled = true,
                Hours = 3,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "about to end" }
            }, null);
            var spoken = CaptureSpeech();
            var timers = container.Resolve<TriggerTimerManager>();

            logParser.Push("You are filled with the power of Aegolism.", DateTime.Now);
            timers.Tick(DateTime.UtcNow.AddSeconds(1));

            CollectionAssert.DoesNotContain(spoken, "about to end");
        }

        // Spell timers have no spell window row to dismiss, so turning the trigger off must stop them.
        [TestMethod]
        public void SpellTriggerTimerStopsWhenTheTriggerIsDisabled()
        {
            var trigger = AddAegolismTimerTrigger(null, new TriggerTimerEnded
            {
                Enabled = true,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "wore off" }
            });
            var spoken = CaptureSpeech();
            var timers = container.Resolve<TriggerTimerManager>();

            logParser.Push("You are filled with the power of Aegolism.", DateTime.Now);
            trigger.TriggerEnabled = false;
            timers.Tick(DateTime.UtcNow.AddHours(3));

            CollectionAssert.DoesNotContain(spoken, "wore off");
        }

        [TestMethod]
        public void SpellTriggerTimerEndedFiresWhenTheSpellRunsOut()
        {
            _ = AddAegolismTimerTrigger(null, new TriggerTimerEnded
            {
                Enabled = true,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "wore off" }
            });
            var spoken = CaptureSpeech();
            var timers = container.Resolve<TriggerTimerManager>();

            logParser.Push("You are filled with the power of Aegolism.", DateTime.Now);
            timers.Tick(DateTime.UtcNow.AddHours(3));

            CollectionAssert.Contains(spoken, "wore off");
        }

        // A resisted counted spell still gets a counter row in the spell window, but it didn't land.
        [TestMethod]
        public void SpellTriggerIgnoresAResistedSpell()
        {
            _ = AddSpellTrigger("Rage of Vallon", SpellTriggerTarget.Anyone, "landed");
            var spoken = CaptureSpeech();
            var d = DateTime.Now;

            logParser.Push("You slash Bristlebane Puppet for 31 points of damage.", d);
            logParser.Push("Your target resisted the Rage of Vallon spell.", d);
            CollectionAssert.DoesNotContain(spoken, "landed");

            logParser.Push("Bristlebane Puppet is weakened by the Rage of Vallon.", d.AddSeconds(1));
            CollectionAssert.Contains(spoken, "landed");
        }

        // Charms never log a landing (it is guessed from the cast) and break with a generic message.
        [TestMethod]
        public void CharmSpellTriggerFiresOnSomeoneElseAndEndsOnTheCharmBreak()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            player.Player.Level = 60;
            var trigger = AddSpellTrigger("Allure", SpellTriggerTarget.Others, "{spell} on {target}");
            trigger.TimerEnded = new TriggerTimerEnded
            {
                Enabled = true,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "{spell} broke" }
            };
            var spoken = CaptureSpeech();
            var d = DateTime.Now;

            logParser.Push("You begin casting Allure.", d);
            logParser.Push("You can't use that command right now...", d.AddSeconds(10));
            CollectionAssert.Contains(spoken, "Allure on your target");

            logParser.Push("Your charm spell has worn off.", d.AddSeconds(30));
            CollectionAssert.Contains(spoken, "Allure broke");
        }

        // The editor pre-fills its Test box with this sample, so it has to actually fire the trigger.
        [TestMethod]
        [DataRow("Frostreaver's Blessing", SpellTriggerTarget.You)]
        [DataRow("Divine Barrier", SpellTriggerTarget.Others)]
        public void SpellTriggerSampleLineFiresTheTrigger(string spellName, SpellTriggerTarget target)
        {
            _ = AddSpellTrigger(spellName, target, "landed");
            var spoken = CaptureSpeech();
            var spell = container.Resolve<EQSpells>().AllSpells[spellName];

            var sample = TriggerTestSampleGenerator.Generate(spell, target);
            Assert.IsFalse(string.IsNullOrEmpty(sample));
            logParser.Push(sample, DateTime.Now);

            CollectionAssert.Contains(spoken, "landed");
        }



    }
}
