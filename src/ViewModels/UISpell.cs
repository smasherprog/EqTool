using EQTool.Models;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.ViewModels
{

    public class UISpell : INotifyPropertyChanged
    {
        public UISpell(DateTime endtime, bool isNPC)
        {
            this.IsNPC = isNPC;
            TimerEndDateTime = endtime;
            TotalSecondsOnSpell = (int)(TimerEndDateTime - DateTime.Now).TotalSeconds;
            UpdateTimeLeft();
        }

        public SpellIcon SpellIcon { get; set; }

        public DateTime UpdatedDateTime { get; set; }

        private TimeSpan _SecondsLeftOnSpell;

        public int TotalSecondsOnSpell { get; private set; }

        public TimeSpan SecondsLeftOnSpell => _SecondsLeftOnSpell;

        private DateTime _TimerEndDateTime = DateTime.Now;
        public DateTime TimerEndDateTime
        {
            get { return _TimerEndDateTime; }
            set
            {
                _TimerEndDateTime = value;
                TotalSecondsOnSpell = (int)(_TimerEndDateTime - DateTime.Now).TotalSeconds;
                UpdateTimeLeft();
            }
        }

        public void UpdateTimeLeft()
        {
            _SecondsLeftOnSpell = TimerEndDateTime - DateTime.Now;
            if (TotalSecondsOnSpell > 0)
            {
                PercentLeftOnSpell = (int)(_SecondsLeftOnSpell.TotalSeconds / TotalSecondsOnSpell * 100);
            }
            OnPropertyChanged(nameof(SecondsLeftOnSpell));
            OnPropertyChanged(nameof(SecondsLeftOnSpellPretty));
            OnPropertyChanged(nameof(PercentLeftOnSpell));
        }

        public bool HasSpellIcon => SpellIcon != null;

        public Int32Rect Rect { get; set; }

        private string _SpellName = string.Empty;

        public string SpellName
        {
            get => _SpellName;
            set
            {
                _SpellName = value;
                OnPropertyChanged();
            }
        }

        public string SpellExtraData
        {
            get
            {
                if (_Counter.HasValue)
                {
                    return " Count --> ";
                }
                else if (Roll >= 0)
                {
                    return $" (#{RollOrder}) Roll --> ";
                }
                return string.Empty;
            }
        }

        public string SpellExtraData2
        {
            get
            {
                if (_Counter.HasValue)
                {
                    return _Counter.Value.ToString();
                }
                else if (Roll >= 0)
                {
                    return Roll.ToString();
                }
                return string.Empty;
            }
        }

        private int? _Counter = null;

        public int? Counter
        {
            get => _Counter;
            set
            {
                _Counter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpellExtraData));
            }
        }

        public bool GuessedSpell { get; set; }

        public Dictionary<PlayerClasses, int> Classes { get; set; } = new Dictionary<PlayerClasses, int>();

        private bool _HideGuesses = true;

        public bool HideGuesses
        {
            get => _HideGuesses;
            set
            {
                if (_HideGuesses == value)
                {
                    return;
                }
                _HideGuesses = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

        private bool _ShowOnlyYou = false;
        public bool ShowOnlyYou
        {
            get => _ShowOnlyYou;
            set
            {
                if (_ShowOnlyYou == value)
                {
                    return;
                }
                _ShowOnlyYou = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

        private bool _HideClasses = true;
        public bool HideClasses
        {
            get => _HideClasses;
            set
            {
                if (_HideClasses == value)
                {
                    return;
                }
                _HideClasses = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

        private Visibility _HeaderVisibility = Visibility.Visible;

        public Visibility HeaderVisibility
        {
            get => _HeaderVisibility;
            set
            {
                if (_HeaderVisibility == value)
                {
                    return;
                }
                _HeaderVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ColumnVisibility
        {
            get
            {
                if (TargetName == CustomTimer.CustomerTime || _SpellType == SpellTypes.Detrimental)
                {
                    return Visibility.Visible;
                }
                else if (_HideGuesses && GuessedSpell)
                {
                    return Visibility.Collapsed;
                }
                else if (_HideClasses)
                {
                    return Visibility.Collapsed;
                }
                else if (_SpellType <= 0 || TargetName == EQSpells.SpaceYou)
                {
                    return Visibility.Visible;
                }
                else if (_ShowOnlyYou && TargetName != EQSpells.SpaceYou)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public string SecondsLeftOnSpellPretty
        {
            get
            {
                var st = "";
                if (_SecondsLeftOnSpell.Hours > 0)
                {
                    st += _SecondsLeftOnSpell.Hours + "h ";
                }
                if (_SecondsLeftOnSpell.Minutes > 0)
                {
                    st += _SecondsLeftOnSpell.Minutes + "m ";
                }
                if (_SecondsLeftOnSpell.Seconds > 0)
                {
                    st += _SecondsLeftOnSpell.Seconds + "s";
                }
                return st;

            }
        }

        public bool PersistentSpell { get; set; }
        public string Sorting
        {
            get
            {
                if (TargetName.StartsWith(" "))
                {
                    return TargetName;
                }
                if (this.Roll >= 0)
                {
                    return " y";
                }
                if (this.IsNPC)
                {
                    return " z";
                }
                return TargetName;
            }
        }
        public string TargetName { get; set; }
        public int Roll { get; set; } = -1;
        public int RollOrder { get; set; } = 0;
        public bool IsNPC { get; set; }

        private SpellTypes _SpellType = 0;

        public SpellTypes SpellType
        {
            get => _SpellType;
            set
            {
                _SpellType = value;
                if (_SpellType == SpellTypes.Beneficial)
                {
                    ProgressBarColor = Brushes.MediumAquamarine;
                }
                else if (_SpellType == SpellTypes.Detrimental)
                {
                    ProgressBarColor = Brushes.OrangeRed;
                }
                else if (_SpellType == SpellTypes.BadGuyCoolDown)
                {
                    ProgressBarColor = Brushes.DarkOrange;
                }
                else if (_SpellType == SpellTypes.HarvestCooldown)
                {
                    ProgressBarColor = Brushes.SkyBlue;
                }
                else if (_SpellType >= SpellTypes.Other)
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
                else if (_SpellType >= SpellTypes.RespawnTimer)
                {
                    ProgressBarColor = Brushes.LightSalmon;
                }
                else if (_SpellType >= SpellTypes.DisciplineCoolDown)
                {
                    ProgressBarColor = Brushes.Gold;
                }
                else
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
            }
        }

        public SolidColorBrush ProgressBarColor { get; set; }

        public int PercentLeftOnSpell { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
