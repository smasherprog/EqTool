using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class EntittyDPS : INotifyPropertyChanged
    {
        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private DateTime _StartTime = DateTime.Now;

        public DateTime StartTime
        {
            get => _StartTime;
            set
            {
                _StartTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DPS));
                OnPropertyChanged(nameof(TotalSeconds));
            }
        }

        public void UpdateDps()
        {
            OnPropertyChanged(nameof(DPS));
            OnPropertyChanged(nameof(TotalSeconds));
        }

        public int TotalSeconds => (int)(DateTime.Now - _StartTime).TotalSeconds;

        private int _TotalDamage = 0;

        public int TotalDamage
        {
            get => _TotalDamage;
            set
            {
                _TotalDamage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DPS));
            }
        }

        public int DPS => (_TotalDamage > 0 && TotalSeconds > 0) ? (int)(_TotalDamage / (double)TotalSeconds) : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
