using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQTool.ViewModels.SettingsComponents;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace EQTool.UI.SettingsComponents
{
    public partial class SettingsTrigger : UserControl
    {
        private readonly TreeTrigger treeTrigger;
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly TriggerHandler triggerHandler;
        private readonly ActivePlayer activePlayer;
        private int validationErrorCount = 0;

        public SettingsTrigger(TreeTrigger treeTrigger, LogParser logParser, LogEvents logEvents, TriggerHandler triggerHandler, ActivePlayer activePlayer)
        {
            this.treeTrigger = treeTrigger;
            this.logParser = logParser;
            this.logEvents = logEvents;
            this.triggerHandler = triggerHandler;
            this.activePlayer = activePlayer;
            DataContext = treeTrigger.Trigger;
            InitializeComponent();
            if (treeTrigger.Trigger is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Trigger_PropertyChanged;
            }

            // For built-in triggers the General Settings inputs (name / search text / regex / zone /
            // category / comments) are fixed; only the Basic / Timer / Timer Ending / Timer Ended /
            // Counter tabs can be edited. Saving a tab edit marks the built-in Customized so its
            // definition isn't overwritten when built-ins are refreshed from code on the next load.
            // The individual inputs bind IsEnabled to IsEditable (rather than disabling the whole
            // group) so the "Copy Regex" button stays usable for read-only built-in triggers.

            // Pre-fill the Test box with a log line that matches this trigger, so the user can
            // click Test straight away and see it fire (and can edit it to try variations).
            TestLogLineText.Text = GenerateTestSample();

            UpdateSaveEnabled();
        }

        private string GenerateTestSample()
        {
            var vm = treeTrigger.Trigger;
            return vm.IsSpellTrigger
                ? TriggerTestSampleGenerator.Generate(vm.SelectedSpell, vm.SpellTarget)
                : TriggerTestSampleGenerator.Generate(vm.Model, activePlayer.Player?.Name);
        }

        private void Trigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // any edit can change overall validity (name/search text)
            UpdateSaveEnabled();

            if (e.PropertyName == nameof(TriggerViewModel.EventType) ||
                e.PropertyName == nameof(TriggerViewModel.SpellName) ||
                e.PropertyName == nameof(TriggerViewModel.SpellTarget))
            {
                // a spell trigger starts with no spell picked, so its sample has to follow the pick
                TestLogLineText.Text = GenerateTestSample();
                TestResultText.Text = string.Empty;
            }

            // A disabled tab that is already selected keeps showing its content, so move off the
            // Timer / Counter tabs when the trigger becomes a spell trigger.
            if (e.PropertyName == nameof(TriggerViewModel.EventType) && treeTrigger.Trigger.IsSpellTrigger &&
                (EditorTabs.SelectedItem == TimerTab || EditorTabs.SelectedItem == CounterTab))
            {
                EditorTabs.SelectedIndex = 0;
            }
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                validationErrorCount++;
            }
            else if (e.Action == ValidationErrorEventAction.Removed && validationErrorCount > 0)
            {
                validationErrorCount--;
            }
            UpdateSaveEnabled();
        }

        // Save is enabled only when the view model reports it is savable AND no number
        // box currently holds an invalid value.
        private void UpdateSaveEnabled()
        {
            var savable = (DataContext as TriggerViewModel)?.IsSavable ?? false;
            SaveButton.IsEnabled = savable && validationErrorCount <= 0;
        }

        // Runs the entered log line through this trigger (regardless of saved/enabled
        // state) using the same runtime services the live trigger handler uses, so the
        // user can verify matching and see/hear the configured outputs and timers.
        private void TestTrigger(object sender, System.Windows.RoutedEventArgs e)
        {
            var line = TestLogLineText.Text?.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                TestResultText.Text = "Enter a log line to test.";
                TestResultText.Foreground = Brushes.Gray;
                return;
            }

            var vm = DataContext as TriggerViewModel;
            var trigger = vm?.Model;
            if (trigger == null)
            {
                return;
            }
            if (activePlayer.Player == null)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(trigger.Zone))
            {
                activePlayer.Player.Zone = trigger.Zone;
            }

            // {c} expands to the current player, as it does when the live handler fires the trigger
            trigger.PlayerName = activePlayer.Player.Name ?? string.Empty;

            // The live handler only fires saved, enabled triggers, and gives each line (or landing)
            // to the first trigger that matches it, so note whether it fires this one while the line
            // runs through the parser (synchronously, since this is the UI thread).
            var firedLive = false;
            void OnTriggerFired(object s, Trigger fired)
            {
                firedLive |= fired == trigger;
            }

            // A spell trigger matches what the spell tracker makes of the line, so also catch the
            // landing it resolves.
            SpellLandedEvent landing = null;
            void OnSpellLanded(object s, SpellLandedEvent landed)
            {
                if (landing == null && landed.Spell != null && trigger.MatchesSpell(landed.Spell.name, landed.TargetName))
                {
                    landing = landed;
                }
            }

            triggerHandler.TriggerFired += OnTriggerFired;
            if (trigger.IsSpellTrigger)
            {
                logEvents.SpellLandedEvent += OnSpellLanded;
            }
            try
            {
                logParser.Push(line, DateTime.Now);
            }
            finally
            {
                triggerHandler.TriggerFired -= OnTriggerFired;
                logEvents.SpellLandedEvent -= OnSpellLanded;
            }

            var matched = trigger.IsSpellTrigger ? landing != null : trigger.Matches(line);
            if (!matched)
            {
                TestResultText.Text = "No match.";
                TestResultText.Foreground = Brushes.DarkRed;
                return;
            }

            if (!firedLive)
            {
                if (landing != null)
                {
                    triggerHandler.FireSpellTrigger(trigger, landing);
                }
                else
                {
                    triggerHandler.FireLogLineTrigger(trigger);
                }
            }

            if (!vm.IsSaved)
            {
                TestResultText.Text = "Matched — outputs/timers fired for this test only. Click Save to make it fire in game.";
                TestResultText.Foreground = Brushes.DarkOrange;
            }
            else if (!trigger.TriggerEnabled)
            {
                TestResultText.Text = "Matched — outputs/timers fired for this test only. Check Trigger Enabled to make it fire in game.";
                TestResultText.Foreground = Brushes.DarkOrange;
            }
            else if (!firedLive)
            {
                TestResultText.Text = "Matched — outputs/timers fired for this test, but the trigger didn't fire on its own (an earlier trigger may match this first).";
                TestResultText.Foreground = Brushes.DarkOrange;
            }
            else
            {
                TestResultText.Text = "Matched — outputs/timers fired.";
                TestResultText.Foreground = Brushes.Green;
            }
        }

        // Copies this trigger's effective regular expression to the clipboard. Copies the converted
        // "real" regex (simplified {name} placeholders expanded into named groups), falling back to
        // the raw search text if a pattern can't be built. Works for built-in triggers too - their
        // General Settings inputs are read-only, but this button is left enabled.
        private void CopyRegex(object sender, System.Windows.RoutedEventArgs e)
        {
            var trigger = (DataContext as TriggerViewModel)?.Model;
            if (trigger == null)
            {
                return;
            }

            string regex;
            try
            {
                regex = trigger.TriggerRegex?.ToString();
            }
            catch (Exception)
            {
                regex = null;
            }
            if (string.IsNullOrEmpty(regex))
            {
                regex = trigger.SearchText;
            }
            if (string.IsNullOrEmpty(regex))
            {
                return;
            }

            try
            {
                System.Windows.Forms.Clipboard.SetText(regex, System.Windows.Forms.TextDataFormat.Text);
                (App.Current as App)?.ShowBalloonTip(2000, "Regex Copied", "The trigger's regular expression has been copied to the clipboard.", System.Windows.Forms.ToolTipIcon.Info);
            }
            catch (Exception)
            {
            }
        }

        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            treeTrigger.Trigger.Save();
        }
    }
}
