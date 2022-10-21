using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

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

        public string TargetName { get; set; }
        private TimeSpan _SecondsLeftOnSpell;
        public TimeSpan SecondsLeftOnSpell
        {
            get => _SecondsLeftOnSpell;
            set
            {
                _SecondsLeftOnSpell = value;
                OnPropertyChanged(nameof(SecondsLeftOnSpellPretty));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
