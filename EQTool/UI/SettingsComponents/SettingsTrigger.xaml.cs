using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels.SettingsComponents;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace EQTool.UI.SettingsComponents
{
    public partial class SettingsTrigger : UserControl
    {
        private readonly TreeTrigger treeTrigger;
        private readonly TriggerActionExecutor executor;
        private readonly TriggerTimerManager timerManager;
        private int validationErrorCount = 0;

        public SettingsTrigger(TreeTrigger treeTrigger, TriggerActionExecutor executor, TriggerTimerManager timerManager)
        {
            this.treeTrigger = treeTrigger;
            this.executor = executor;
            this.timerManager = timerManager;
            DataContext = treeTrigger.Trigger;
            InitializeComponent();
            if (treeTrigger.Trigger is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Trigger_PropertyChanged;
            }
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

            // let any active timers from this trigger end early on the line, just like live
            timerManager.OnLine(line);

            if (trigger.Matches(line))
            {
                executor.Execute(trigger.GetEffectiveBasic(), trigger.Expand);
                if (trigger.Timer != null && trigger.Timer.IsEnabled)
                {
                    timerManager.HandleTimerMatch(trigger);
                }
                if (trigger.Counter != null && trigger.Counter.ResetEnabled)
                {
                    timerManager.HandleCounterMatch(trigger);
                }
                TestResultText.Text = "Matched — outputs/timers fired.";
                TestResultText.Foreground = Brushes.Green;
            }
            else
            {
                TestResultText.Text = "No match.";
                TestResultText.Foreground = Brushes.DarkRed;
            }
        }

        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            this.treeTrigger.Trigger.Save();
        }
    }
}
