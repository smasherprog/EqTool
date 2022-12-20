using EQTool.Models;
using EQTool.Services.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.ViewModels
{
    public class EQNameValue
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class SettingsWindowData : INotifyPropertyChanged
    {

        public SettingsWindowData()
        {
            for (var i = 12; i < 72; i++)
            {
                FontSizes.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            for (var i = 1; i < 61; i++)
            {
                Levels.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }

            foreach(var item in ZoneParser.Zones)
            {
                this.Zones.Add(item);
            }
        }

        private string _EqPath = string.Empty;
        public string EqPath
        {
            get => _EqPath;
            set
            {
                _EqPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DoesNotHaveEqPath));
                OnPropertyChanged(nameof(HasEqPath));
                OnPropertyChanged(nameof(MissingConfiguration));
                OnPropertyChanged(nameof(NotMissingConfiguration));
            }
        }

        public bool DoesNotHaveEqPath => string.IsNullOrWhiteSpace(_EqPath);
        public bool HasEqPath => !string.IsNullOrWhiteSpace(_EqPath);

        private bool _IsLogginEnabled = false;

        public bool IsLogginEnabled
        {
            get => _IsLogginEnabled;
            set
            {
                _IsLogginEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLogginDisabled));
                OnPropertyChanged(nameof(MissingConfiguration));
                OnPropertyChanged(nameof(NotMissingConfiguration));
            }
        }

        public bool IsLogginDisabled => !_IsLogginEnabled;

        public bool MissingConfiguration => DoesNotHaveEqPath || IsLogginDisabled;
        public bool NotMissingConfiguration => HasEqPath && IsLogginEnabled;

        private string _CharName = string.Empty;
        public string CharName
        {
            get => _CharName;
            set
            {
                _CharName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCharName));
                OnPropertyChanged(nameof(HasNoCharName));
            }
        }

        private string _Zone = string.Empty;
        public string Zone
        {
            get => _Zone;
            set
            {
                _Zone = value;
                OnPropertyChanged(); 
            }
        }

        public bool HasCharName => !string.IsNullOrWhiteSpace(_CharName);
        public Visibility HasNoCharName => string.IsNullOrWhiteSpace(_CharName) ? Visibility.Visible : Visibility.Collapsed;

        private int _CharLevel = 1;
        public int CharLevel
        {
            get => _CharLevel;
            set
            {
                _CharLevel = value;
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

        public List<PlayerClasses> PlayerClasses => Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().ToList();

        public bool EqRunning
        {
            get => IsEqNotRunning;
            set
            {
                IsEqNotRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEqNotRunning));
            }
        }

        public bool TestingMode
        {
            get
            {
                var ret = true;
#if DEBUG
                ret = true;
#else

                ret = false;
#endif
                return ret;
            }
        }
        public bool IsEqRunning => !IsEqNotRunning;
        public bool IsEqNotRunning { get; private set; } = false;

        public ObservableCollection<string> Zones = new ObservableCollection<string>();
        public ObservableCollection<EQNameValue> FontSizes = new ObservableCollection<EQNameValue>();
        public ObservableCollection<EQNameValue> Levels = new ObservableCollection<EQNameValue>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
