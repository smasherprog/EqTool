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
            if (trigger.TimerEnding == null)
            {
                trigger.TimerEnding = new TriggerTimerEnding();
            }
            if (trigger.TimerEnded == null)
            {
                trigger.TimerEnded = new TriggerTimerEnded();
            }
            if (trigger.Counter == null)
            {
                trigger.Counter = new TriggerCounter();
            }
            if (string.IsNullOrEmpty(trigger.Category))
            {
                trigger.Category = "Default";
            }
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
                OnPropertyChanged(nameof(CanUseFastCheck));
                RefreshValidation();
            }
        }

        public bool UseFastCheck
        {
            get => trigger.UseFastCheck;
            set { trigger.UseFastCheck = value; OnPropertyChanged(); }
        }

        public bool CanUseFastCheck => !UseRegex;

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
            new KeyValuePair<TimerType, string>(TimerType.CountUp, "Timer (Count Up)"),
            new KeyValuePair<TimerType, string>(TimerType.Stopwatch, "Stopwatch"),
            new KeyValuePair<TimerType, string>(TimerType.RepeatingTimer, "Repeating Timer"),
        };

        public List<KeyValuePair<TimerRestartBehavior, string>> RestartBehaviorOptions => new List<KeyValuePair<TimerRestartBehavior, string>>
        {
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.StartNewTimer, "Start a new timer"),
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.RestartTimer, "Restart current timer"),
            new KeyValuePair<TimerRestartBehavior, string>(TimerRestartBehavior.RestartTimerIfRunning, "Restart timer if running"),
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
            OnPropertyChanged(nameof(IsSavable));
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

        public bool IsSavable =>
            string.IsNullOrWhiteSpace(TriggerNameErrorMessge) &&
            string.IsNullOrWhiteSpace(SearchTextErrorMessge);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
