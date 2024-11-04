using EQTool.Models;
using EQTool.Services.Handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using static EQTool.Services.Handlers.SpawnTimerTrigger;

namespace EQTool.ViewModels
{
    public class SpawnTimerDialogViewModel : INotifyPropertyChanged
    {
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

        //
        // timer start fields
        //
        private bool _PigParseAI            = false;
        private bool _ExpMessage            = false;
        private bool _SlainMessage          = false;
        private bool _FactionMessage        = false;
        private string _SlainText           = "";
        private string _FactionText         = "";

        public bool PigParseAI
        {
            get { return _PigParseAI; }
            set
            {
                _PigParseAI = value;
                OnPropertyChanged();
            }
        }

        public bool ExpMessage
        {
            get { return _ExpMessage; }
            set
            {
                _ExpMessage = value;
                OnPropertyChanged();
            }
        }

        public bool SlainMessage
        {
            get { return _SlainMessage; }
            set
            {
                _SlainMessage = value;
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

        public bool FactionMessage
        {
            get { return _FactionMessage; }
            set
            {
                _FactionMessage = value;
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


        //
        // timer end fields
        //
        private string _WarningTime         = "";
        
        private bool _ProvideWarningText    = false;
        private bool _ProvideWarningTTS     = false;
        private string _WarningText         = "";
        private string _WarningTTS          = "";

        private bool _ProvideEndText        = false;
        private bool _ProvideEndTTS         = false;
        private string _EndText             = "";
        private string _EndTTS              = "";


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


        //
        // counter reset field
        //
        private string _CounterResetTime = "";

        public string CounterResetTime
        {
            get { return _CounterResetTime; }
            set
            {
                _CounterResetTime = value;
                OnPropertyChanged();
            }
        }


        //
        // timer duration fields
        //
        private bool _Preset0600        = false;
        private bool _Preset0640        = false;
        private bool _Preset1430        = false;
        private bool _Preset2200        = false;
        private bool _Preset2800        = false;
        private bool _Custom            = false;
        private string _CustomDuration  = "";

        public bool Preset0600
        {
            get { return _Preset0600; }
            set
            {
                _Preset0600 = value;
                OnPropertyChanged();
            }
        }

        public bool Preset0640
        {
            get { return _Preset0640; }
            set
            {
                _Preset0640 = value;
                OnPropertyChanged();
            }
        }

        public bool Preset1430
        {
            get { return _Preset1430; }
            set
            {
                _Preset1430 = value;
                OnPropertyChanged();
            }
        }

        public bool Preset2200
        {
            get{ return _Preset2200; }
            set
            {
                _Preset2200 = value;
                OnPropertyChanged();
            }
        }

        public bool Preset2800
        {
            get { return _Preset2800; }
            set
            {
                _Preset2800 = value;
                OnPropertyChanged();
            }
        }

        public bool Custom
        {
            get { return _Custom; }
            set
            {
                _Custom = value;
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


        //
        // notes and comments
        //
        // todo - add in the proper VM field for a RichTextBox




        //private bool _expMessageSelected = false;

        //public bool ExpMessageSelected
        //{
        //    get { return _expMessageSelected; }
        //    set
        //    {
        //        this._expMessageSelected = value;
        //        SlainMessage = "Disabled";
        //        SlainMessageColor = Brushes.Red; 
        //        this.OnPropertyChanged(nameof(SlainMessageColor));
        //        this.OnPropertyChanged(); 
        //    }
        //}

        //private string _slainMessage = string.Empty;

        //public string SlainMessage
        //{
        //    get { return _slainMessage; }
        //    set
        //    {
        //        this._slainMessage = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private bool _slainMessageSelected = false;

        //public bool SlainMessageSelected
        //{
        //    get { return _slainMessageSelected; }
        //    set
        //    {
        //        this._slainMessageSelected = value;
        //        SlainMessageColor = Brushes.Blue;
        //        this.SlainMessage = "Enter your message here";
        //        this.OnPropertyChanged();
        //        this.OnPropertyChanged(nameof(SlainMessageColor));
        //    }
        //}

        //public SolidColorBrush SlainMessageColor { get; set; } = Brushes.Black;

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

            // vm doesn't know about enums
            PigParseAI = false;
            ExpMessage = false;
            SlainMessage = false;
            FactionMessage = false;

            switch (m.StartType)
            {
                case StartTypes.PIG_PARSE_AI:
                    PigParseAI = true;
                    break;
                case StartTypes.EXP_MESSAGE:
                    ExpMessage = true;
                    break;
                case StartTypes.SLAIN_MESSAGE:
                    SlainMessage = true;
                    break;
                case StartTypes.FACTION_MESSAGE:
                    FactionMessage |= true;
                    break;
            }

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

            // vm doesn't know about enums
            Preset0600 = false;
            Preset0640 = false;
            Preset1430 = false;
            Preset2200 = false;
            Preset2800 = false;
            Custom = false;

            switch (m.Duration)
            {
                case Durations.PRESET_0600:
                    Preset0600 = true;
                    break;
                case Durations.PRESET_0640:
                    Preset0640 = true;
                    break;
                case Durations.PRESET_1430:
                    Preset1430 = true;
                    break;
                case Durations.PRESET_2200:
                    Preset2200 = true;
                    break;
                case Durations.PRESET_2800:
                    Preset2800 = true;
                    break;
                case Durations.CUSTOM:
                    Custom = true;
                    break;
            }

            CustomDuration = m.CustomDuration;

            //
            // notes and comments field
            //
            // todo - add in the proper field for a RichTextBox


        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // button click handlers
        internal void DoSomething_DontTryTopassData_ItShouldBeInTheeViewModelAllready()
        {
            //do something here with the button click. Maybe get the value of the text box and do an api call?
            //var slainmessage = this.SlainMessage;

        }
    }
}
