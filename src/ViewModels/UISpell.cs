using EQTool.Models;
using EQToolShared.Enums;
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
        public UISpell(DateTime endtime)
        {
            _TimerEndDateTime = endtime;
            TotalSecondsOnSpell = (int)(_TimerEndDateTime - DateTime.Now).TotalSeconds;
            UpdateTimeLeft();
        }

        public SpellIcon SpellIcon { get; set; }

        public DateTime UpdatedDateTime { get; set; }

        private TimeSpan _SecondsLeftOnSpell;

        public int TotalSecondsOnSpell { get; private set; }

        public TimeSpan SecondsLeftOnSpell => _SecondsLeftOnSpell;

        private readonly DateTime _TimerEndDateTime;

        public void UpdateTimeLeft()
        {
            _SecondsLeftOnSpell = _TimerEndDateTime - DateTime.Now;
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

        public string SpellExtraData => _SieveCounter.HasValue ? " --> Sieves: " + _SieveCounter.Value : string.Empty;

        private int? _SieveCounter = null;

        public int? SieveCounter
        {
            get => _SieveCounter;
            set
            {
                _SieveCounter = value;
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
                if (_SpellType <= 0 || TargetName == EQSpells.SpaceYou)
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
                    st += _SecondsLeftOnSpell.Hours + " hr ";
                }
                if (_SecondsLeftOnSpell.Minutes > 0)
                {
                    st += _SecondsLeftOnSpell.Minutes + " m ";
                }
                if (_SecondsLeftOnSpell.Seconds > 0)
                {
                    st += _SecondsLeftOnSpell.Seconds + " s";
                }
                return st;

            }
        }

        public bool PersistentSpell { get; set; }

        public string TargetName { get; set; }

        private int _SpellType = 0;
        public int SpellType
        {
            get => _SpellType;
            set
            {
                _SpellType = value;
                ProgressBarColor = _SpellType == -1 ? Brushes.LightBlue : (_SpellType > 0 ? Brushes.DarkSeaGreen : Brushes.OrangeRed);
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
