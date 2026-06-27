using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.Models
{
    // View model backing the trigger editor. Binds directly to a live
    // Trigger instance (and its sub-objects) and persists on Save.
    public class TriggerViewModel : INotifyPropertyChanged
    {
        private readonly Trigger trigger;
        private readonly EQToolSettings toolSettings;
        private readonly EQToolSettingsLoad settingsLoad;
        private bool IsNewTrigger = false;

        public TriggerViewModel(Trigger trigger, EQToolSettings toolSettings, EQToolSettingsLoad settingsLoad)
        {
            this.trigger = trigger;
            this.toolSettings = toolSettings;
            this.settingsLoad = settingsLoad;
            EnsureComponents();
            RefreshValidation();
        }

        public TriggerViewModel(EQToolSettings toolSettings, EQToolSettingsLoad settingsLoad)
        {
            IsNewTrigger = true;
            this.trigger = new Trigger
            {
                TriggerId = Guid.NewGuid(),
                TriggerEnabled = false,
                TriggerName = "New Trigger",
                SearchText = string.Empty,
                Category = "Default",
                UseRegex = false
            };
            this.toolSettings = toolSettings;
            this.settingsLoad = settingsLoad;
            EnsureComponents();
            RefreshValidation();
        }

        // Make sure every component object exists (and migrate legacy triggers so the
        // Basic tab is populated from the old DisplayText/AudioText fields).
        private void EnsureComponents()
        {
            if (trigger.Basic == null)
            {
                trigger.Basic = trigger.GetEffectiveBasic();
            }
            if (trigger.UseRegex == null)
            {
                // legacy triggers were always regex
                trigger.UseRegex = true;
            }
            if (trigger.Timer == null)
            {
                trigger.Timer = new TriggerTimer();
            }
            // "Count Up" and "Stopwatch" are no longer offered; coerce any trigger that still
            // carries one of those to Count Down so the dropdown shows a valid option instead of a
            // blank selection. ("No Timer" is still a valid option, so it is left untouched.)
            if (trigger.Timer.TimerType == TimerType.CountUp || trigger.Timer.TimerType == TimerType.Stopwatch)
            {
                trigger.Timer.TimerType = TimerType.CountDown;
            }
            // The old "Restart timer if running" behavior was removed; a trigger persisted with it
            // now holds an undefined value, so fall back to "Start a new timer" for the dropdown.
            if (!Enum.IsDefined(typeof(TimerRestartBehavior), trigger.Timer.RestartBehavior))
            {
                trigger.Timer.RestartBehavior = TimerRestartBehavior.StartNewTimer;
            }
            if (trigger.TimerEnding == null)
            {
                trigger.TimerEnding = new TriggerTimerEnding();
            }
            if (trigger.TimerEnding.Output == null)
            {
                trigger.TimerEnding.Output = new TriggerOutput();
            }
            if (trigger.TimerEnded == null)
            {
                trigger.TimerEnded = new TriggerTimerEnded();
            }
            if (trigger.TimerEnded.Output == null)
            {
                trigger.TimerEnded.Output = new TriggerOutput();
            }
            if (trigger.Counter == null)
            {
                trigger.Counter = new TriggerCounter();
            }
            if (string.IsNullOrEmpty(trigger.Category))
            {
                trigger.Category = "Default";
            }

            // Re-validate live as the user edits the sub-objects bound in the tabs. These are bound
            // directly (their DataContext is the sub-object, not this view model), so their changes
            // would not otherwise reach the editor's validation.
            trigger.Basic.PropertyChanged += Component_PropertyChanged;
            trigger.Timer.PropertyChanged += Component_PropertyChanged;
            trigger.TimerEnding.PropertyChanged += Component_PropertyChanged;
            trigger.TimerEnding.Output.PropertyChanged += Component_PropertyChanged;
            trigger.TimerEnded.PropertyChanged += Component_PropertyChanged;
            trigger.TimerEnded.Output.PropertyChanged += Component_PropertyChanged;
            trigger.Counter.PropertyChanged += Component_PropertyChanged;
        }

        public void Save()
        {
            if (trigger.IsBuiltIn)
            {
                // built-in library triggers are never persisted or modified
                return;
            }
            if (IsNewTrigger)
            {
                toolSettings.Triggers.Add(trigger);
                IsNewTrigger = false;
            }
            settingsLoad.Save(toolSettings);
        }

        // Structural property set by the Triggers tree (folder placement).
        public Guid? FolderId
        {
            get => trigger.FolderId;
            set => trigger.FolderId = value;
        }

        public Guid TriggerId => trigger.TriggerId;

        // The underlying trigger model (used by the editor's Test feature).
        public Trigger Model => trigger;

        // Built-in (library) triggers are read-only until copied out.
        public bool IsBuiltIn => trigger.IsBuiltIn;
        public bool IsEditable => !trigger.IsBuiltIn;

        public string TriggerName
        {
            get => trigger.TriggerName;
            set
            {
                trigger.TriggerName = value;
                OnPropertyChanged();
                RefreshValidation();
            }
        }

        public bool TriggerEnabled
        {
            get => trigger.TriggerEnabled;
            set { trigger.TriggerEnabled = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => trigger.SearchText;
            set
            {
                trigger.SearchText = value;
                OnPropertyChanged();
                RefreshValidation();
            }
        }

        public bool UseRegex
        {
            get => trigger.EffectiveUseRegex;
            set
            {
                trigger.UseRegex = value;
                OnPropertyChanged();
                RefreshValidation();
            }
        }

        public string Category
        {
            get => trigger.Category;
            set { trigger.Category = value; OnPropertyChanged(); }
        }

        public string Comments
        {
            get => trigger.Comments;
            set { trigger.Comments = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Categories
        {
            get
            {
                var cats = toolSettings.Triggers
                    .Select(a => a.Category)
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(a => a)
                    .ToList();
                if (!cats.Any(a => string.Equals(a, "Default", StringComparison.OrdinalIgnoreCase)))
                {
                    cats.Insert(0, "Default");
                }
                return new ObservableCollection<string>(cats);
            }
        }

        // Component objects bound directly by the editor tabs.
        public TriggerOutput Basic => trigger.Basic;
        public TriggerTimer Timer => trigger.Timer;
        public TriggerTimerEnding TimerEnding => trigger.TimerEnding;
        public TriggerTimerEnded TimerEnded => trigger.TimerEnded;
        public TriggerCounter Counter => trigger.Counter;

        public List<KeyValuePair<TimerType, string>> TimerTypeOptions => new List<KeyValuePair<TimerType, string>>
        {
            new KeyValuePair<TimerType, string>(TimerType.NoTimer, "No Timer"),
            new KeyValuePair<TimerType, string>(TimerType.CountDown, "Timer (Count Down)"),
            new KeyValuePair<TimerType, string>(TimerType.RepeatingTimer, "Repeating Timer"),
        };

        public List<KeyValuePair<TimerRestartBehavior, string>> RestartBehaviorOptions => new List<KeyValuePair<TimerRestartBehavior, string>>
        {
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.StartNewTimer, "Start a new timer"),
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.RestartTimer, "Restart current timer"),
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.DoNothing, "Do nothing"),
        };

        // ---- Validation ----

        private void RefreshValidation()
        {
            OnPropertyChanged(nameof(TriggerNameErrorMessge));
            OnPropertyChanged(nameof(TriggerNameErrorMessgeVisible));
            OnPropertyChanged(nameof(TriggerNameBorderBrush));
            OnPropertyChanged(nameof(SearchTextErrorMessge));
            OnPropertyChanged(nameof(SearchTextErrorMessgeVisible));
            OnPropertyChanged(nameof(SearchTextBorderBrush));
            OnPropertyChanged(nameof(DisplayTextErrorMessge));
            OnPropertyChanged(nameof(DisplayTextErrorMessgeVisible));
            OnPropertyChanged(nameof(TimerErrorMessge));
            OnPropertyChanged(nameof(TimerErrorMessgeVisible));
            OnPropertyChanged(nameof(TimerEndingErrorMessge));
            OnPropertyChanged(nameof(TimerEndingErrorMessgeVisible));
            OnPropertyChanged(nameof(TimerEndedErrorMessge));
            OnPropertyChanged(nameof(TimerEndedErrorMessgeVisible));
            OnPropertyChanged(nameof(CounterErrorMessge));
            OnPropertyChanged(nameof(CounterErrorMessgeVisible));
            OnPropertyChanged(nameof(TimerNameBorderBrush));
            OnPropertyChanged(nameof(TimerDurationBorderBrush));
            OnPropertyChanged(nameof(TimerEndingBorderBrush));
            OnPropertyChanged(nameof(CounterDurationBorderBrush));
            OnPropertyChanged(nameof(BasicTabHasError));
            OnPropertyChanged(nameof(TimerTabHasError));
            OnPropertyChanged(nameof(TimerEndingTabHasError));
            OnPropertyChanged(nameof(TimerEndedTabHasError));
            OnPropertyChanged(nameof(CounterTabHasError));
            OnPropertyChanged(nameof(ValidationSummary));
            OnPropertyChanged(nameof(ValidationSummaryVisible));
            OnPropertyChanged(nameof(IsSavable));
        }

        // Fires whenever a bound sub-object (Basic/Timer/Timer Ending/Timer Ended) changes so the
        // editor re-evaluates validity as the user types.
        private void Component_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshValidation();
        }

        public string TriggerNameErrorMessge =>
            string.IsNullOrWhiteSpace(trigger.TriggerName) ? "Trigger Name is required." : string.Empty;

        public Visibility TriggerNameErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(TriggerNameErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        public Brush TriggerNameBorderBrush =>
            string.IsNullOrWhiteSpace(TriggerNameErrorMessge) ? Brushes.Gray : Brushes.Red;

        public string SearchTextErrorMessge
        {
            get
            {
                if (string.IsNullOrWhiteSpace(trigger.SearchText))
                {
                    return "Search Text is required.";
                }
                if (UseRegex)
                {
                    try
                    {
                        var r = trigger.TriggerRegex;
                        if (r == null)
                        {
                            return "There was an error creating the regex!";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                return string.Empty;
            }
        }

        public Visibility SearchTextErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(SearchTextErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        public Brush SearchTextBorderBrush =>
            string.IsNullOrWhiteSpace(SearchTextErrorMessge) ? Brushes.Gray : Brushes.Red;

        // Basic tab: a display must have text when its checkbox is enabled.
        public string DisplayTextErrorMessge
        {
            get
            {
                var basic = trigger.Basic;
                return basic != null && basic.DisplayTextEnabled && string.IsNullOrWhiteSpace(basic.DisplayText)
                    ? "Display Text is required when \"Display Text\" is checked."
                    : string.Empty;
            }
        }

        public Visibility DisplayTextErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(DisplayTextErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        // Timer tab: when a timer is selected it needs a name and a non-zero duration.
        public string TimerErrorMessge
        {
            get
            {
                var timer = trigger.Timer;
                if (timer == null || timer.TimerType == TimerType.NoTimer)
                {
                    return string.Empty;
                }
                if (string.IsNullOrWhiteSpace(timer.TimerName))
                {
                    return "Timer Name is required when a timer is selected.";
                }
                if (timer.Duration.TotalMilliseconds <= 0)
                {
                    return "Timer Duration must be greater than zero when a timer is selected.";
                }
                return string.Empty;
            }
        }

        public Visibility TimerErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(TimerErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        // Timer Ending tab: when enabled the "notify when down to" time cannot be all zeros, and it
        // must actually do something (display text or audio).
        public string TimerEndingErrorMessge
        {
            get
            {
                var ending = trigger.TimerEnding;
                if (ending == null || !ending.Enabled)
                {
                    return string.Empty;
                }
                if (ending.Threshold.TotalMilliseconds <= 0)
                {
                    return "Timer Ending time must be greater than zero when enabled.";
                }
                if (!HasOutput(ending.Output))
                {
                    return "Timer Ending must have display text or audio configured when enabled.";
                }
                return string.Empty;
            }
        }

        public Visibility TimerEndingErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(TimerEndingErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        // Timer Ended tab: when enabled it must actually do something (display text or audio).
        public string TimerEndedErrorMessge
        {
            get
            {
                var ended = trigger.TimerEnded;
                return ended != null && ended.Enabled && !HasOutput(ended.Output)
                    ? "Timer Ended must have display text or audio configured when enabled."
                    : string.Empty;
            }
        }

        public Visibility TimerEndedErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(TimerEndedErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        // Counter tab: the reset time cannot be all zeros when the reset checkbox is selected.
        public string CounterErrorMessge
        {
            get
            {
                var counter = trigger.Counter;
                return counter != null && counter.ResetEnabled && counter.ResetAfter.TotalMilliseconds <= 0
                    ? "Counter reset time must be greater than zero when reset is enabled."
                    : string.Empty;
            }
        }

        public Visibility CounterErrorMessgeVisible =>
            string.IsNullOrWhiteSpace(CounterErrorMessge) ? Visibility.Collapsed : Visibility.Visible;

        // True when the output would actually show text or play audio (matches what the executor does).
        private static bool HasOutput(TriggerOutput o)
        {
            if (o == null)
            {
                return false;
            }
            if (o.DisplayTextEnabled && !string.IsNullOrWhiteSpace(o.DisplayText))
            {
                return true;
            }
            if (o.AudioType == TriggerAudioType.TextToSpeech && !string.IsNullOrWhiteSpace(o.TtsText))
            {
                return true;
            }
            if (o.AudioType == TriggerAudioType.SoundFile && !string.IsNullOrWhiteSpace(o.SoundFile))
            {
                return true;
            }
            return false;
        }

        // Red borders for the specific invalid controls (mirrors Trigger Name / Search Text).
        private bool TimerSelected => trigger.Timer != null && trigger.Timer.TimerType != TimerType.NoTimer;

        public Brush TimerNameBorderBrush =>
            TimerSelected && string.IsNullOrWhiteSpace(trigger.Timer.TimerName) ? Brushes.Red : Brushes.Gray;

        public Brush TimerDurationBorderBrush =>
            TimerSelected && trigger.Timer.Duration.TotalMilliseconds <= 0 ? Brushes.Red : Brushes.Gray;

        public Brush TimerEndingBorderBrush =>
            trigger.TimerEnding != null && trigger.TimerEnding.Enabled && trigger.TimerEnding.Threshold.TotalMilliseconds <= 0
                ? Brushes.Red : Brushes.Gray;

        public Brush CounterDurationBorderBrush =>
            trigger.Counter != null && trigger.Counter.ResetEnabled && trigger.Counter.ResetAfter.TotalMilliseconds <= 0
                ? Brushes.Red : Brushes.Gray;

        // Per-tab error flags so each tab header can highlight when that tab needs attention.
        public bool BasicTabHasError => !string.IsNullOrWhiteSpace(DisplayTextErrorMessge);
        public bool TimerTabHasError => !string.IsNullOrWhiteSpace(TimerErrorMessge);
        public bool TimerEndingTabHasError => !string.IsNullOrWhiteSpace(TimerEndingErrorMessge);
        public bool TimerEndedTabHasError => !string.IsNullOrWhiteSpace(TimerEndedErrorMessge);
        public bool CounterTabHasError => !string.IsNullOrWhiteSpace(CounterErrorMessge);

        // Combined list of the tab-based validation errors, shown next to the Save button so the user
        // can see why saving is blocked even when the offending field is on a tab that isn't open.
        // (Trigger Name / Search Text are excluded; they already show inline at the top of the form.)
        public string ValidationSummary
        {
            get
            {
                var errors = new[]
                {
                    DisplayTextErrorMessge,
                    TimerErrorMessge,
                    TimerEndingErrorMessge,
                    TimerEndedErrorMessge,
                    CounterErrorMessge
                }.Where(e => !string.IsNullOrWhiteSpace(e));
                return string.Join(Environment.NewLine, errors);
            }
        }

        public Visibility ValidationSummaryVisible =>
            string.IsNullOrWhiteSpace(ValidationSummary) ? Visibility.Collapsed : Visibility.Visible;

        public bool IsSavable =>
            string.IsNullOrWhiteSpace(TriggerNameErrorMessge) &&
            string.IsNullOrWhiteSpace(SearchTextErrorMessge) &&
            string.IsNullOrWhiteSpace(DisplayTextErrorMessge) &&
            string.IsNullOrWhiteSpace(TimerErrorMessge) &&
            string.IsNullOrWhiteSpace(TimerEndingErrorMessge) &&
            string.IsNullOrWhiteSpace(TimerEndedErrorMessge) &&
            string.IsNullOrWhiteSpace(CounterErrorMessge);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
