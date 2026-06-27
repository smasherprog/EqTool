using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.Models
{
    // How a trigger output produces audio.
    public enum TriggerAudioType
    {
        None,
        TextToSpeech,
        SoundFile
    }

    // The kind of timer a trigger creates when it matches.
    public enum TimerType
    {
        NoTimer,
        CountDown,
        CountUp,
        Stopwatch,
        RepeatingTimer
    }

    // What to do when a trigger matches again while its timer is already running.
    // Values are explicit so removing the old RestartTimerIfRunning (=2) doesn't renumber the
    // others — existing saved triggers persist these as integers.
    public enum TimerRestartBehavior
    {
        StartNewTimer = 0,
        RestartTimer = 1,
        DoNothing = 3
    }

    // Common base for the small editable models so two-way binding updates the UI.
    [Serializable]
    public abstract class TriggerNotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
    }

    // The set of actions performed when something fires: optionally show overlay text,
    // copy text to the clipboard, and play audio (TTS or a sound file).
    // Reused by the Basic tab, Timer Ending tab, and Timer Ended tab.
    [Serializable]
    public class TriggerOutput : TriggerNotifyBase
    {
        private bool _DisplayTextEnabled;
        public bool DisplayTextEnabled { get => _DisplayTextEnabled; set => Set(ref _DisplayTextEnabled, value); }

        private string _DisplayText = string.Empty;
        public string DisplayText { get => _DisplayText; set => Set(ref _DisplayText, value); }

        private string _DisplayTextColor = "Red";
        public string DisplayTextColor { get => _DisplayTextColor; set => Set(ref _DisplayTextColor, value); }

        private TriggerAudioType _AudioType = TriggerAudioType.None;
        public TriggerAudioType AudioType
        {
            get => _AudioType;
            set
            {
                if (_AudioType != value)
                {
                    _AudioType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNoSound));
                    OnPropertyChanged(nameof(IsTextToSpeech));
                    OnPropertyChanged(nameof(IsSoundFile));
                }
            }
        }

        private string _TtsText = string.Empty;
        public string TtsText { get => _TtsText; set => Set(ref _TtsText, value); }

        private bool _InterruptSpeech;
        public bool InterruptSpeech { get => _InterruptSpeech; set => Set(ref _InterruptSpeech, value); }

        private string _SoundFile = string.Empty;
        public string SoundFile { get => _SoundFile; set => Set(ref _SoundFile, value); }

        // Helpers for binding the three mutually-exclusive audio radio buttons.
        [Newtonsoft.Json.JsonIgnore]
        public bool IsNoSound
        {
            get => AudioType == TriggerAudioType.None;
            set { if (value) { AudioType = TriggerAudioType.None; } }
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsTextToSpeech
        {
            get => AudioType == TriggerAudioType.TextToSpeech;
            set { if (value) { AudioType = TriggerAudioType.TextToSpeech; } }
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsSoundFile
        {
            get => AudioType == TriggerAudioType.SoundFile;
            set { if (value) { AudioType = TriggerAudioType.SoundFile; } }
        }
    }

    // A single "end early" search pattern for a timer.
    [Serializable]
    public class EndEarlyEntry : TriggerNotifyBase
    {
        private string _SearchText = string.Empty;
        public string SearchText { get => _SearchText; set => Set(ref _SearchText, value); }

        private bool _UseRegex;
        public bool UseRegex { get => _UseRegex; set => Set(ref _UseRegex, value); }
    }

    // The Timer tab settings.
    [Serializable]
    public class TriggerTimer : TriggerNotifyBase
    {
        private TimerType _TimerType = TimerType.CountDown;
        public TimerType TimerType { get => _TimerType; set => Set(ref _TimerType, value); }

        private string _TimerName = string.Empty;
        public string TimerName { get => _TimerName; set => Set(ref _TimerName, value); }

        private int _Hours;
        public int Hours { get => _Hours; set => Set(ref _Hours, value); }

        private int _Minutes;
        public int Minutes { get => _Minutes; set => Set(ref _Minutes, value); }

        private int _Seconds;
        public int Seconds { get => _Seconds; set => Set(ref _Seconds, value); }

        private int _Milliseconds;
        public int Milliseconds { get => _Milliseconds; set => Set(ref _Milliseconds, value); }

        private TimerRestartBehavior _RestartBehavior = TimerRestartBehavior.StartNewTimer;
        public TimerRestartBehavior RestartBehavior { get => _RestartBehavior; set => Set(ref _RestartBehavior, value); }

        // Color of the timer's progress bar in the Triggers (spell) window.
        private string _BarColor = "MediumPurple";
        public string BarColor { get => _BarColor; set => Set(ref _BarColor, value); }

        // Name of the spell whose icon is shown next to this timer. Defaults to Feign Death.
        private string _IconName = "Feign Death";
        public string IconName { get => _IconName; set => Set(ref _IconName, value); }

        // Also mirror this timer's countdown as an animated progress bar in the overlay window.
        private bool _ShowInOverlay;
        public bool ShowInOverlay { get => _ShowInOverlay; set => Set(ref _ShowInOverlay, value); }

        public System.Collections.ObjectModel.ObservableCollection<EndEarlyEntry> EndEarlyTexts { get; set; }
            = new System.Collections.ObjectModel.ObservableCollection<EndEarlyEntry>();

        [Newtonsoft.Json.JsonIgnore]
        public TimeSpan Duration => new TimeSpan(0, Hours, Minutes, Seconds, Milliseconds);

        [Newtonsoft.Json.JsonIgnore]
        public bool IsEnabled => TimerType != TimerType.NoTimer;
    }

    // The Timer Ending tab: notify when the timer counts down to a threshold.
    [Serializable]
    public class TriggerTimerEnding : TriggerNotifyBase
    {
        private bool _Enabled;
        public bool Enabled { get => _Enabled; set => Set(ref _Enabled, value); }

        private int _Hours;
        public int Hours { get => _Hours; set => Set(ref _Hours, value); }

        private int _Minutes;
        public int Minutes { get => _Minutes; set => Set(ref _Minutes, value); }

        private int _Seconds = 1;
        public int Seconds { get => _Seconds; set => Set(ref _Seconds, value); }

        public TriggerOutput Output { get; set; } = new TriggerOutput();

        [Newtonsoft.Json.JsonIgnore]
        public TimeSpan Threshold => new TimeSpan(0, Hours, Minutes, Seconds);
    }

    // The Timer Ended tab: notify when the timer reaches zero.
    [Serializable]
    public class TriggerTimerEnded : TriggerNotifyBase
    {
        private bool _Enabled;
        public bool Enabled { get => _Enabled; set => Set(ref _Enabled, value); }

        public TriggerOutput Output { get; set; } = new TriggerOutput();
    }

    // The Counter tab: optionally reset the match counter after a period of inactivity.
    // Enabling reset also enables counting/display for this trigger.
    [Serializable]
    public class TriggerCounter : TriggerNotifyBase
    {
        private bool _ResetEnabled;
        public bool ResetEnabled { get => _ResetEnabled; set => Set(ref _ResetEnabled, value); }

        private int _Hours;
        public int Hours { get => _Hours; set => Set(ref _Hours, value); }

        private int _Minutes;
        public int Minutes { get => _Minutes; set => Set(ref _Minutes, value); }

        private int _Seconds;
        public int Seconds { get => _Seconds; set => Set(ref _Seconds, value); }

        [Newtonsoft.Json.JsonIgnore]
        public TimeSpan ResetAfter => new TimeSpan(0, Hours, Minutes, Seconds);
    }
}
