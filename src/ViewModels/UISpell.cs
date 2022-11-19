using EQTool.Models;
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
        public SpellIcon SpellIcon { get; set; }

        public bool HasSpellIcon => SpellIcon != null;

        public Int32Rect Rect { get; set; }

        public string SpellName { get; set; }

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
                OnPropertyChanged(nameof(ColumnVisiblity));
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
                OnPropertyChanged(nameof(ColumnVisiblity));
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
                OnPropertyChanged(nameof(ColumnVisiblity));
            }
        }

        private bool _Collapse = false;
        public bool Collapse
        {
            get => _Collapse;
            set
            {
                if (_Collapse == value)
                {
                    return;
                }
                _Collapse = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisiblity));
                OnPropertyChanged(nameof(SortingOrder));
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

        public Visibility ColumnVisiblity
        {
            get
            {
                if (_HideGuesses && GuessedSpell)
                {
                    return Visibility.Collapsed;
                }
                else if (_HideClasses || _Collapse)
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

        public int TotalSecondsOnSpell { get; set; }

        public string SortingOrder => Collapse ? "~~ " + TargetName + "~~" : TargetName;
        public string TargetName { get; set; }

        private TimeSpan _SecondsLeftOnSpell;

        public TimeSpan SecondsLeftOnSpell
        {
            get => _SecondsLeftOnSpell;
            set
            {
                _SecondsLeftOnSpell = value;
                if (TotalSecondsOnSpell > 0)
                {
                    PercentLeftOnSpell = (int)(_SecondsLeftOnSpell.TotalSeconds / TotalSecondsOnSpell * 100);
                }
                OnPropertyChanged(nameof(SecondsLeftOnSpellPretty));
                OnPropertyChanged(nameof(PercentLeftOnSpell));
            }
        }

        private int _SpellType = 0;
        public int SpellType
        {
            get => _SpellType;
            set
            {
                _SpellType = value;
                ProgressBarColor = _SpellType > 0 ? Brushes.DarkSeaGreen : Brushes.OrangeRed;
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
