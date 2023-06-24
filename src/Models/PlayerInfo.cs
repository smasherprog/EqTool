using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.Models
{
    public class SessionPlayerDamage : INotifyPropertyChanged
    {
        private PlayerDamage _CurrentSessionPlayerDamage;
        public PlayerDamage CurrentSessionPlayerDamage
        {
            get
            {
                if (_CurrentSessionPlayerDamage == null)
                {
                    _CurrentSessionPlayerDamage = new PlayerDamage();
                }
                return _CurrentSessionPlayerDamage;
            }
            set
            {
                _CurrentSessionPlayerDamage = value ?? new PlayerDamage();
                OnPropertyChanged();
            }
        }

        public Visibility LastSessionPlayerDamageVisability => _LastSessionPlayerDamage != null ? Visibility.Visible : Visibility.Collapsed;

        private PlayerDamage _LastSessionPlayerDamage;
        public PlayerDamage LastSessionPlayerDamage
        {
            get => _LastSessionPlayerDamage;
            set
            {
                _LastSessionPlayerDamage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastSessionPlayerDamageVisability));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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
                if (_HighestHit >= 32000)
                {
                    _HighestHit = 0;
                }

                if (value >= 32000)
                {
                    return;
                }

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

    public class YouSpells
    {
        public string Name { get; set; }
        public int TotalSecondsLeft { get; set; }
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
                if (_BestPlayerDamage == null)
                {
                    _BestPlayerDamage = new PlayerDamage();
                }
                return _BestPlayerDamage;
            }
            set
            {
                _BestPlayerDamage = value ?? new PlayerDamage();
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

        private string _GuildName;
        public string GuildName
        {
            get => _GuildName;
            set
            {
                _GuildName = value;
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

        private Servers _Server;
        public Servers Server
        {
            get => _Server;
            set
            {
                _Server = value;
                OnPropertyChanged();
            }
        }

        private List<YouSpells> _YouSpells;

        public List<YouSpells> YouSpells
        {
            get
            {
                if (_YouSpells == null)
                {
                    _YouSpells = new List<YouSpells>();
                }
                return _YouSpells;
            }
            set
            {
                _YouSpells = value ?? new List<YouSpells>();
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
