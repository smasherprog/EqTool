using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.Models
{
    public class EQNameValue
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class SettingsWindowData : INotifyPropertyChanged
    {
        private string _EqPath = string.Empty;
        public string EqPath
        {
            get => _EqPath;
            set
            {
                _EqPath = value;
                OnPropertyChanged(nameof(EqPath));
            }
        }

        private string _CharName = string.Empty;
        public string CharName
        {
            get => _CharName;
            set
            {
                _CharName = value;
                OnPropertyChanged(nameof(CharName));
            }
        }
        private int _CharLevel = 1;
        public int CharLevel
        {
            get => _CharLevel;
            set
            {
                _CharLevel = value;
                OnPropertyChanged(nameof(CharLevel));
            }
        }

        public bool EqRunning
        {
            get => IsEqNotRunning;
            set
            {
                IsEqNotRunning = value;
                OnPropertyChanged(nameof(IsEqRunning));
                OnPropertyChanged(nameof(IsEqNotRunning));
            }
        }

        public bool IsEqRunning => !IsEqNotRunning;
        public bool IsEqNotRunning { get; private set; } = false;

        private bool _IsLogginEnabled = false;

        public bool IsLogginEnabled
        {
            get => _IsLogginEnabled;
            set
            {
                _IsLogginEnabled = value;
                OnPropertyChanged(nameof(IsLogginEnabled));
                OnPropertyChanged(nameof(IsLogginDisabled));
            }
        }

        public bool IsLogginDisabled => !_IsLogginEnabled;

        public ObservableCollection<EQNameValue> FontSizes = new ObservableCollection<EQNameValue>();
        public ObservableCollection<EQNameValue> Levels = new ObservableCollection<EQNameValue>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
