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

        private Trigger AddSpellTrigger(string spellName, string audioText)
        {
            var trigger = new Trigger
            {
                TriggerEnabled = true,
                TriggerName = "Spell Trigger",
                EventType = TriggerEventType.Spell,
                SpellName = spellName,
                AudioTextEnabled = audioText != null,
                AudioText = audioText
            };
            container.Resolve<EQToolSettings>().Triggers.Add(trigger);
            return trigger;
        }

        [TestMethod]
        public void SpellTriggerFiresWhenSpellLandsOnYou()
        {
            _ = AddSpellTrigger("Frostreaver's Blessing", "{spell} landed");
            var spoken = CaptureSpeech();

            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);

            CollectionAssert.Contains(spoken, "Frostreaver's Blessing landed");
        }

        [TestMethod]
        public void SpellTriggerIgnoresLandingsOnSomeoneElse()
        {
            _ = AddSpellTrigger("Divine Barrier", "landed");
            var spoken = CaptureSpeech();

            logParser.Push("Someone is surrounded by a divine barrier.", DateTime.Now);

            CollectionAssert.DoesNotContain(spoken, "landed");
        }

        // A trigger switched from Log Text to Spell keeps its old search text; it must not match on it.
        [TestMethod]
        public void SpellTriggerIgnoresItsSearchText()
        {
            var trigger = AddSpellTrigger("Frostreaver's Blessing", "landed");
            trigger.SearchText = "^Can you hear me now?";
            var spoken = CaptureSpeech();

            logParser.Push("Can you hear me now?", DateTime.Now);

            CollectionAssert.DoesNotContain(spoken, "landed");
        }

        [TestMethod]
        public void SpellTriggerTimerEndedFiresWhenTheSpellWearsOff()
        {
            player.Player.Level = 60;
            var trigger = AddSpellTrigger("Aegolism", null);
            trigger.TimerEnded = new TriggerTimerEnded
            {
                Enabled = true,
                Output = new TriggerOutput { AudioType = TriggerAudioType.TextToSpeech, TtsText = "{spell} wore off" }
            };
            var spoken = CaptureSpeech();

            logParser.Push("You are filled with the power of Aegolism.", DateTime.Now);
            Assert.IsEmpty(spoken);
            logParser.Push("Your aegolism fades.", DateTime.Now.AddSeconds(40));

            CollectionAssert.Contains(spoken, "Aegolism wore off");
        }

        [TestMethod]
        public void SpellTriggerTimerEndingShowsOverlayText()
        {
            player.Player.Level = 60;
            var trigger = AddSpellTrigger("Aegolism", null);
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
            var trigger = AddSpellTrigger("Aegolism", null);
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

        [TestMethod]
        public void CharmSpellTriggerIgnoresTheLandingGuessedFromTheCast()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Enchanter;
            player.Player.Level = 60;
            _ = AddSpellTrigger("Allure", "landed");
            var spoken = CaptureSpeech();
            var d = DateTime.Now;

            logParser.Push("You begin casting Allure.", d);
            logParser.Push("You can't use that command right now...", d.AddSeconds(10));

            CollectionAssert.DoesNotContain(spoken, "landed");
        }

        // The editor pre-fills its Test box with this sample, so it has to actually fire the trigger.
        [TestMethod]
        public void SpellTriggerSampleLineFiresTheTrigger()
        {
            _ = AddSpellTrigger("Frostreaver's Blessing", "landed");
            var spoken = CaptureSpeech();
            var spell = container.Resolve<EQSpells>().AllSpells["Frostreaver's Blessing"];

            var sample = TriggerTestSampleGenerator.Generate(spell);
            Assert.IsFalse(string.IsNullOrEmpty(sample));
            logParser.Push(sample, DateTime.Now);

            CollectionAssert.Contains(spoken, "landed");
        }



    }
}
