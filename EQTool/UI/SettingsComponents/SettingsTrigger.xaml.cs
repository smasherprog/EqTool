using EQTool.Models;
using EQTool.Services;
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
        private readonly ActivePlayer activePlayer;
        private int validationErrorCount = 0;

        public SettingsTrigger(TreeTrigger treeTrigger, LogParser logParser, ActivePlayer activePlayer)
        {
            this.treeTrigger = treeTrigger;
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            DataContext = treeTrigger.Trigger;
            InitializeComponent();
            if (treeTrigger.Trigger is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Trigger_PropertyChanged;
            }

            // For built-in triggers the General Settings (name / search text / regex / zone /
            // category / comments) are fixed; only the Basic / Timer / Timer Ending / Timer Ended /
            // Counter tabs can be edited. Saving a tab edit marks the built-in Customized so its
            // definition isn't overwritten when built-ins are refreshed from code on the next load.
            if (treeTrigger.Trigger.IsBuiltIn)
            {
                GeneralSettingsGroup.IsEnabled = false;
            }

            // Pre-fill the Test box with a log line that matches this trigger, so the user can
            // click Test straight away and see it fire (and can edit it to try variations).
            TestLogLineText.Text = TriggerTestSampleGenerator.Generate(treeTrigger.Trigger.Model, activePlayer.Player?.Name);

            UpdateSaveEnabled();
        }

        private void Trigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // any edit can change overall validity (name/search text)
            UpdateSaveEnabled();
        }

        // Fired when a bound field's ValidationRule adds or removes an error.
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

            var trigger = (DataContext as TriggerViewModel)?.Model;
            if (trigger == null)
            {
                return;
            }
            var oldZone = activePlayer.Player.Zone;
            if (!string.IsNullOrWhiteSpace(trigger.Zone))
            {
                activePlayer.Player.Zone = trigger.Zone;
            }
            logParser.Push(line, DateTime.Now);
            if (trigger.Matches(line))
            {
                TestResultText.Text = "Matched — outputs/timers fired.";
                TestResultText.Foreground = Brushes.Green;
            }
            else
            {
                TestResultText.Text = "No match.";
                TestResultText.Foreground = Brushes.DarkRed;
            }
            activePlayer.Player.Zone = oldZone;
        }

        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            treeTrigger.Trigger.Save();
        }
    }
}
