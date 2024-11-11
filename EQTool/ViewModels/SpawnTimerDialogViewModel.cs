using EQTool.UI;
using EQTool.Models;
using EQTool.Services.Handlers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using static EQTool.Services.Handlers.SpawnTimerTrigger;
using System;
using System.Globalization;

namespace EQTool.ViewModels
{

    public class SpawnTimerDialogViewModel : INotifyPropertyChanged
    {
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

        public string CounterResetTime
        {
            get { return _CounterResetTime; }
            set
            {
                _CounterResetTime = value;
                OnPropertyChanged();
            }
        }

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


        // utility function to set all fields to the corresponding field in the Model class
        // todo - instead of laboriously converting between the VM and the M, can we just use same class for both?
        public void SetFrom(SpawnTimerTrigger m)
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
            StartType = m.StartType;

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
