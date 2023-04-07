using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.Models
{
    public class SessionPlayerDamage : PlayerDamage
    {
    }

    [Serializable]
    public class PlayerDamage : INotifyPropertyChanged
    {
        private int _TargetTotalDamage;
        public int TargetTotalDamage
        {
            get => _TargetTotalDamage;
            set
            {
                _TargetTotalDamage = value;
                OnPropertyChanged();
            }
        }

        private int _HighestDPS = 0;
        public int HighestDPS
        {
            get => _HighestDPS;
            set
            {
                _HighestDPS = value;
                OnPropertyChanged();
            }
        }

        private int _HighestHit = 0;
        public int HighestHit
        {
            get => _HighestHit;
            set
            {
                _HighestHit = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    [Serializable]
    public class PlayerInfo : INotifyPropertyChanged
    {
        private int _Level = 1;
        public int Level
        {
            get => _Level;
            set
            {
                _Level = value;
                OnPropertyChanged();
            }
        }

        private PlayerDamage _BestPlayerDamage;

        public PlayerDamage BestPlayerDamage
        {
            get
            {
                if (this._BestPlayerDamage == null)
                {
                    this._BestPlayerDamage = new PlayerDamage();
                }
                return _BestPlayerDamage;
            }
            set
            {
                if (value == null)
                {
                    this._BestPlayerDamage = new PlayerDamage();
                }
                else
                {
                    _BestPlayerDamage = value;
                }
                OnPropertyChanged();
            }
        }

        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private string _Zone;
        public string Zone
        {
            get => _Zone;
            set
            {
                _Zone = value;
                OnPropertyChanged();
            }
        }

        private PlayerClasses? _PlayerClass;
        public PlayerClasses? PlayerClass
        {
            get => _PlayerClass;
            set
            {
                _PlayerClass = value;
                OnPropertyChanged();
            }
        }

        public List<PlayerClasses> ShowSpellsForClasses { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
