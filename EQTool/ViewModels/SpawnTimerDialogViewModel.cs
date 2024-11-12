using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EQTool.ViewModels
{

    public class SpawnTimerDialogViewModel : INotifyPropertyChanged
    {
        private const string hmsPattern =
            @"^(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)";
        //    ^ ((?<hh>[0-9]+):)?                                        Group "hh"      0 or more (sets of numbers, followed by a colon)
        //                       ((?<mm>[0-9]+):)                        Group "mm"      1 set of numbers followed by a colon
        //     (                                 )?                                      0 or more hh and mm groups
        //                                         (?<ss>[0-9]+)         Group "ss"      1 set of numbers
        //
        // https://regex101.com/r/oBV1NI/1
        //
        private readonly Regex regex = new Regex(hmsPattern, RegexOptions.Compiled);


        // -----------------------------------------------------------------------
        //
        // the overall enable/disable Spawn Timers checkbox
        //
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

        // -----------------------------------------------------------------------
        //
        // timer start fields
        //

        //
        // enum for starttypes
        //
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

        //
        // timer start getters and setters
        //
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

        // -----------------------------------------------------------------------
        //
        // timer end fields
        //
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

        //
        // timer end getters and setters
        //
        public string WarningTime
        {
            get { return _WarningTime; }
            set
            {
                _WarningTime = value;
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

        // -----------------------------------------------------------------------
        //
        // counter reset field
        //
        private string _CounterResetTime = "1:00:00";
        private int _CounterResetSeconds = 3600;
        public int TimerCounter { get; set; } = 0;  // todo - need logic to reset this to 0 after reset time is exceeded

        public string CounterResetTime
        {
            get { return _CounterResetTime; }
            set
            {
                _CounterResetTime = value;
                OnPropertyChanged();
            }
        }
        public int CounterResetSeconds { get { return _WarningSeconds; } }


        // -----------------------------------------------------------------------
        //
        // timer duration fields
        //

        //
        // enum for durations
        //
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

        //
        // timer duration getters and setters
        //
        public Durations Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;
                OnPropertyChanged();
            }
        }
        public int DurationSeconds { get { return _DurationSeconds; } }

        public string CustomDuration
        {
            get { return _CustomDuration; }
            set
            {
                _CustomDuration = value;
                OnPropertyChanged();
            }
        }

        // -----------------------------------------------------------------------
        //
        // notes and comments
        //
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

        //
        // utility function to set all fields to the same value as the passed object fields
        // apparently C Sharp does not allow overloading operator= ?
        //
        // we will also use this function to calculate and set the xxxxSeconds fields for the 
        // hh:mm:ss time fields
        //
        public void SetFrom(SpawnTimerDialogViewModel m)
        {
            // sweep thru all the fields

            //
            // overall enable/disable
            //
            SpawnTimerEnabled = m.SpawnTimerEnabled;

            //
            // timer start
            //
            StartType = m.StartType;
            SlainText = m.SlainText;
            FactionText = m.FactionText;

            //
            // timer end
            //
            WarningTime = m.WarningTime;

            ProvideWarningText = m.ProvideWarningText;
            ProvideWarningTTS = m.ProvideWarningTTS;
            WarningText = m.WarningText;
            WarningTTS = m.WarningTTS;

            ProvideEndText = m.ProvideEndText;
            ProvideEndTTS = m.ProvideEndTTS;
            EndText = m.EndText;
            EndTTS = m.EndTTS;

            //
            // counter reset field
            //
            CounterResetTime = m.CounterResetTime;

            //
            // timer duration fields
            //
            Duration = m.Duration;
            CustomDuration = m.CustomDuration;

            //
            // notes and comments field
            //
            NotesText = m.NotesText;


            // calculate the number of seconds for the Duration field
            switch (Duration)
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
                        // get results from the rexex scan
                        var hh = match_duration.Groups["hh"].Value;
                        var mm = match_duration.Groups["mm"].Value;
                        var ss = match_duration.Groups["ss"].Value;

                        // count up the seconds
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
                    break;
            }

            // calculate the number of seconds in the warning time field
            var match_warning = regex.Match(WarningTime);
            if (match_warning.Success)
            {
                // get results from the rexex scan
                var hh = match_warning.Groups["hh"].Value;
                var mm = match_warning.Groups["mm"].Value;
                var ss = match_warning.Groups["ss"].Value;

                // count up the seconds
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

            // calculate the number of seconds in the counter reset field
            var match_counter = regex.Match(CounterResetTime);
            if (match_counter.Success)
            {
                // get results from the rexex scan
                var hh = match_counter.Groups["hh"].Value;
                var mm = match_counter.Groups["mm"].Value;
                var ss = match_counter.Groups["ss"].Value;

                // count up the seconds
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    //
    // converter class to allow radiobuttons to bind to enum
    //
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
