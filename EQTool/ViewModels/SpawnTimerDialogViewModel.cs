using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace EQTool.ViewModels
{

    public class SpawnTimerDialogViewModel : INotifyPropertyChanged
    {
        // Matches hh:mm:ss, mm:ss, or ss - the leading hh and mm groups are optional as a pair,
        // so a lone number parses as seconds. https://regex101.com/r/oBV1NI/1
        private const string hmsPattern =
            @"^(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)$";
        private readonly Regex regex = new Regex(hmsPattern, RegexOptions.Compiled);


        private bool _SpawnTimerEnabled = false;
        public bool SpawnTimerEnabled
        {
            get { return _SpawnTimerEnabled; }
            set
            {
                _SpawnTimerEnabled = value;
                OnPropertyChanged();
            }
        }

        public enum StartTypes
        {
            PIG_PARSE_AI,
            EXP_MESSAGE,
            SLAIN_MESSAGE,
            FACTION_MESSAGE
        }

        private StartTypes _StartType   = StartTypes.EXP_MESSAGE;
        private string _SlainText       = "(an ancient cyclops|a pirate|a cyclops|Boog Mudtoe)";
        private string _FactionText     = "(Coldain|Rygorr)";

        public StartTypes StartType
        {
            get { return _StartType; }
            set
            {
                _StartType = value;
                OnPropertyChanged();
            }
        }

        public string SlainText
        {
            get { return _SlainText; }
            set
            {
                _SlainText = value;
                OnPropertyChanged();
            }
        }

        public string FactionText
        {
            get { return _FactionText; }
            set
            {
                _FactionText = value;
                OnPropertyChanged();
            }
        }

        private string _WarningTime         = "30";
        private int _WarningSeconds         = 30;

        private bool _ProvideWarningText    = true;
        private bool _ProvideWarningTTS     = true;
        private string _WarningText         = "30 second warning";
        private string _WarningTTS          = "30 second warning";

        private bool _ProvideEndText        = true;
        private bool _ProvideEndTTS         = true;
        private string _EndText             = "Pop";
        private string _EndTTS              = "Pop";

        public string WarningTime
        {
            get { return _WarningTime; }
            set
            {
                _WarningTime = value;

                var match_warning = regex.Match(_WarningTime);
                if (match_warning.Success)
                {
                    var hh = match_warning.Groups["hh"].Value;
                    var mm = match_warning.Groups["mm"].Value;
                    var ss = match_warning.Groups["ss"].Value;

                    _WarningSeconds = 0;
                    if (ss != "")
                    {
                        _WarningSeconds += int.Parse(ss);
                    }
                    if (mm != "")
                    {
                        _WarningSeconds += 60 * int.Parse(mm);
                    }
                    if (hh != "")
                    {
                        _WarningSeconds += 3600 * int.Parse(hh);
                    }
                }
                else
                {
                    _WarningSeconds = 0;
                }

                OnPropertyChanged();
            }
        }
        public int WarningSeconds { get { return _WarningSeconds; } }


        public bool ProvideWarningText
        {
            get { return _ProvideWarningText; }
            set
            {
                _ProvideWarningText = value;
                OnPropertyChanged();
            }
        }

        public bool ProvideWarningTTS
        {
            get { return _ProvideWarningTTS; }
            set
            {
                _ProvideWarningTTS = value;
                OnPropertyChanged();
            }
        }

        public string WarningText
        {
            get { return _WarningText; }
            set
            {
                _WarningText = value;
                OnPropertyChanged();
            }
        }

        public string WarningTTS
        {
            get { return _WarningTTS; }
            set
            {
                _WarningTTS = value;
                OnPropertyChanged();
            }
        }

        public bool ProvideEndText
        {
            get { return _ProvideEndText; }
            set
            {
                _ProvideEndText = value;
                OnPropertyChanged();
            }
        }

        public bool ProvideEndTTS
        {
            get { return _ProvideEndTTS; }
            set
            {
                _ProvideEndTTS = value;
                OnPropertyChanged();
            }
        }

        public string EndText
        {
            get { return _EndText; }
            set
            {
                _EndText = value;
                OnPropertyChanged();
            }
        }

        public string EndTTS
        {
            get { return _EndTTS; }
            set
            {
                _EndTTS = value;
                OnPropertyChanged();
            }
        }

        private string      _CounterResetTime = "1:00:00";
        private int         _CounterResetSeconds = 3600;
        public int          TimerCounter { get; set; } = 0;
        public DateTime     LastUsedTime { get; set; } = DateTime.Now;

        public string CounterResetTime
        {
            get { return _CounterResetTime; }
            set
            {
                _CounterResetTime = value;

                var match_counter = regex.Match(_CounterResetTime);
                if (match_counter.Success)
                {
                    var hh = match_counter.Groups["hh"].Value;
                    var mm = match_counter.Groups["mm"].Value;
                    var ss = match_counter.Groups["ss"].Value;

                    _CounterResetSeconds = 0;
                    if (ss != "")
                    {
                        _CounterResetSeconds += int.Parse(ss);
                    }
                    if (mm != "")
                    {
                        _CounterResetSeconds += 60 * int.Parse(mm);
                    }
                    if (hh != "")
                    {
                        _CounterResetSeconds += 3600 * int.Parse(hh);
                    }
                }
                else
                {
                    _CounterResetSeconds = 0;
                }

                OnPropertyChanged();
            }
        }

        public int CounterResetSeconds { get { return _CounterResetSeconds; } }


        public int GetNextTimerCounter()
        {
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - LastUsedTime;
            if (timeSpan.TotalSeconds > CounterResetSeconds)
                TimerCounter = 0;

            LastUsedTime = now;
            return ++TimerCounter;
        }


        public enum Durations
        {
            PRESET_0600,
            PRESET_0640,
            PRESET_1430,
            PRESET_2200,
            PRESET_2800,
            CUSTOM
        }

        private Durations _Duration     = Durations.CUSTOM;
        private string _CustomDuration  = "30:00";
        private int _DurationSeconds    = 1800;

        public Durations Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;

                switch (_Duration)
                {
                    case Durations.PRESET_0600:
                        _DurationSeconds = 6 * 60;
                        break;

                    case Durations.PRESET_0640:
                        _DurationSeconds = 6 * 60 + 40;
                        break;

                    case Durations.PRESET_1430:
                        _DurationSeconds = 14 * 60 + 30;
                        break;

                    case Durations.PRESET_2200:
                        _DurationSeconds = 22 * 60;
                        break;

                    case Durations.PRESET_2800:
                        _DurationSeconds = 28 * 60;
                        break;

                    case Durations.CUSTOM:
                        var match_duration = regex.Match(CustomDuration);
                        if (match_duration.Success)
                        {
                            var hh = match_duration.Groups["hh"].Value;
                            var mm = match_duration.Groups["mm"].Value;
                            var ss = match_duration.Groups["ss"].Value;

                            _DurationSeconds = 0;
                            if (ss != "")
                            {
                                _DurationSeconds += int.Parse(ss);
                            }
                            if (mm != "")
                            {
                                _DurationSeconds += 60 * int.Parse(mm);
                            }
                            if (hh != "")
                            {
                                _DurationSeconds += 3600 * int.Parse(hh);
                            }
                        }
                        else
                        {
                            _DurationSeconds = 0;
                        }

                        break;
                }

                OnPropertyChanged();
            }
        }

        public string CustomDuration
        {
            get { return _CustomDuration; }
            set
            {
                _CustomDuration = value;

                if (Duration == Durations.CUSTOM)
                {
                    var match_duration = regex.Match(_CustomDuration);
                    if (match_duration.Success)
                    {
                        var hh = match_duration.Groups["hh"].Value;
                        var mm = match_duration.Groups["mm"].Value;
                        var ss = match_duration.Groups["ss"].Value;

                        _DurationSeconds = 0;
                        if (ss != "")
                        {
                            _DurationSeconds += int.Parse(ss);
                        }
                        if (mm != "")
                        {
                            _DurationSeconds += 60 * int.Parse(mm);
                        }
                        if (hh != "")
                        {
                            _DurationSeconds += 3600 * int.Parse(hh);
                        }
                    }
                    else
                    {
                        _DurationSeconds = 0;
                    }
                }

                OnPropertyChanged();
            }
        }
        public int DurationSeconds { get { return _DurationSeconds; } }


        private string _NotesText =
            "AC in OOT: 6 min\r\n" +
            "(an ancient cyclops|a pirate|a cyclops|Boog Mudtoe)\r\n" +
            "\r\n" +
            "Drelzna: 19 min\r\n" +
            "(a necromancer|Drelzna)\r\n" +
            "\r\n" +
            "Zone respawns\r\n" +
            "Najena 19 min, Skyfire 13 min, EW 6:40\r\n" +
            "Oasis specs = 16:30, OOT specs/sisters = 6:00\r\n" +
            "lower guk = 28:00, Grobb = 24:00, Kedge = 22:00\r\n" +
            "WK guards = 6:00, BB fishers 6:40 and 22:00\r\n" +
            "North Felwithe guards = 24:00, Paw = 22:00\r\n" +
            "MM = 23:00, Droga = 20:30, HS = 20:30\r\n" +
            "Perma = 22:00, TT = 6:40, TD = 12:00, Skyshrine = 30:00\r\n" +
            "Skyfire = 13:00, Seb Lair = 27:00, Hole = 21:30\r\n" +
            "Wars Woods brutes = 6:40, Skyshrine = 30:00\r\n" +
            "Chardok = 18:00, Crystal Caverns = 14:45, COM = 22:00\r\n" +
            "Kael =28:00, WL = 14:30\r\n" +
            "\r\n";

        public string NotesText
        {
            get { return _NotesText; }
            set
            {
                _NotesText = value;
                OnPropertyChanged();
            }
        }



        public void SetFrom(SpawnTimerDialogViewModel m)
        {
            SpawnTimerEnabled = m.SpawnTimerEnabled;

            StartType = m.StartType;
            SlainText = m.SlainText;
            FactionText = m.FactionText;

            WarningTime = m.WarningTime;

            ProvideWarningText = m.ProvideWarningText;
            ProvideWarningTTS = m.ProvideWarningTTS;
            WarningText = m.WarningText;
            WarningTTS = m.WarningTTS;

            ProvideEndText = m.ProvideEndText;
            ProvideEndTTS = m.ProvideEndTTS;
            EndText = m.EndText;
            EndTTS = m.EndTTS;

            CounterResetTime = m.CounterResetTime;
            TimerCounter = m.TimerCounter;
            LastUsedTime = m.LastUsedTime;

            Duration = m.Duration;
            CustomDuration = m.CustomDuration;

            NotesText = m.NotesText;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class HourMinutesSecondsValidationRule : ValidationRule
    {
        private const string hmsPattern = @"^(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)$";
        private readonly Regex regex = new Regex(hmsPattern, RegexOptions.Compiled);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var match = regex.Match(value.ToString());
            if (match.Success)
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "Invalid time input.  Valid input formats = 'hours:minutes:seconds', or 'minutes:seconds', or 'seconds'");
        }
    }

    // Lets radiobuttons bind to an enum: the button's parameter is compared to the bound value.
    public class RadioButtonEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
