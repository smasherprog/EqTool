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

    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public SettingsWindowViewModel(ActivePlayer activePlayer)
        {
            ActivePlayer = activePlayer;
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
                Levels.Add(i);
            }

            foreach (var item in ZoneParser.Zones)
            {
                Zones.Add(item);
            }
        }

        private ActivePlayer _ActivePlayer;
        public ActivePlayer ActivePlayer
        {
            get => _ActivePlayer;
            set
            {
                _ActivePlayer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCharName));
                OnPropertyChanged(nameof(HasNoCharName));
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

        private bool _IsLoggingEnabled = false;

        public bool IsLoggingEnabled
        {
            get => _IsLoggingEnabled;
            set
            {
                _IsLoggingEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoggingDisabled));
                OnPropertyChanged(nameof(MissingConfiguration));
                OnPropertyChanged(nameof(NotMissingConfiguration));
            }
        }

        public bool IsLoggingDisabled => !_IsLoggingEnabled;

        public bool MissingConfiguration => DoesNotHaveEqPath || IsLoggingDisabled;
        public bool NotMissingConfiguration => HasEqPath && IsLoggingEnabled;

        public bool HasCharName => !string.IsNullOrWhiteSpace(ActivePlayer?.Player?.Name);
        public Visibility HasNoCharName => string.IsNullOrWhiteSpace(ActivePlayer?.Player?.Name) ? Visibility.Visible : Visibility.Collapsed;

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
        public ObservableCollection<int> Levels = new ObservableCollection<int>();

        public void Update()
        {
            _ = ActivePlayer.Update();
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
