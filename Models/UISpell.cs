using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.Models
{
    public class UISpell : INotifyPropertyChanged
    {
        public SpellIcon SpellIcon { get; set; }
        public bool HasSpellIcon => SpellIcon != null;
        public Int32Rect Rect { get; set; }
        public string SpellName { get; set; }

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
