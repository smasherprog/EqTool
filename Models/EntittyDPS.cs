using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.Models
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

        private int _TotalSeconds = 0;

        public int TotalSeconds
        {
            get => _TotalSeconds;
            set
            {
                _TotalSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DPS));
            }
        }


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

        public int DPS => (_TotalDamage > 0 && _TotalSeconds > 0) ? (_TotalDamage / _TotalSeconds) : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
