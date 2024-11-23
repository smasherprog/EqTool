using EQTool.Models;
using EQToolShared.Enums;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.ViewModels
{
    public class BoolStringClass : INotifyPropertyChanged
    {
        public string TheText { get; set; }

        public PlayerClasses TheValue { get; set; }

        private bool _IsChecked { get; set; }

        public bool IsChecked
        {
            get => _IsChecked; set
            {
                _IsChecked = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private readonly EQToolSettings toolSettings;

        public SettingsWindowViewModel(ActivePlayer activePlayer, EQToolSettings toolSettings)
        {
            this.toolSettings = toolSettings;
            ActivePlayer = activePlayer;
            for (var i = 12; i < 72; i++)
            {
                FontSizes.Add(i);
            }
            for (var i = 1; i < 61; i++)
            {
                Levels.Add(i);
            }

            for (var i = 1; i < 201; i++)
            {
                TrackSkills.Add(i);
            }

            foreach (var item in EQToolShared.Zones.ZoneNames.OrderBy(a => a))
            {
                Zones.Add(item);
            }

            foreach (var item in Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>())
            {
                var listitem = new BoolStringClass { TheText = item.ToString(), TheValue = item, IsChecked = false };
                SelectedPlayerClasses.Add(listitem);
            }
            InstalledVoices = new ObservableCollection<string>();
#if !LINUX
            InstalledVoices = new ObservableCollection<string>(new System.Speech.Synthesis.SpeechSynthesizer().GetInstalledVoices().Select(a => a.VoiceInfo.Name).ToList());
#endif

            SelectedVoice = this.toolSettings.SelectedVoice;
            if (string.IsNullOrWhiteSpace(SelectedVoice))
            {
                SelectedVoice = InstalledVoices.FirstOrDefault();
            }
        }

        public int GlobalFontSize
        {
            get => toolSettings.FontSize.Value;
            set
            {
                toolSettings.FontSize = value;
                App.Current.Resources["GlobalFontSize"] = (double)value;
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleDPS", toolSettings.DpsWindowState.Opacity.Value);
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleMap", toolSettings.MapWindowState.Opacity.Value);
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleTrigger", toolSettings.SpellWindowState.Opacity.Value);
                OnPropertyChanged();
            }
        }

        public bool RaidModeDetection
        {
            get => toolSettings.DpsWindowState.AlwaysOnTop;
            set
            {
                toolSettings.DpsWindowState.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public bool DpsAlwaysOnTop
        {
            get => toolSettings.DpsWindowState.AlwaysOnTop;
            set
            {
                toolSettings.DpsWindowState.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public double DPSWindowOpacity
        {
            get => toolSettings.DpsWindowState.Opacity ?? 1.0;
            set
            {
                toolSettings.DpsWindowState.Opacity = value;
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleDPS", value);
                OnPropertyChanged();
            }
        }

        public bool MapAlwaysOnTop
        {
            get => toolSettings.MapWindowState.AlwaysOnTop;
            set
            {
                toolSettings.MapWindowState.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public double MapWindowOpacity
        {
            get => toolSettings.MapWindowState.Opacity ?? 1.0;

            set
            {
                toolSettings.MapWindowState.Opacity = value;
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleMap", value);
                OnPropertyChanged();
            }
        }
        public bool MobAlwaysOnTop
        {
            get => toolSettings.MobWindowState.AlwaysOnTop;
            set
            {
                toolSettings.MobWindowState.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public bool SpellAlwaysOnTop
        {
            get => toolSettings.SpellWindowState.AlwaysOnTop;
            set
            {
                toolSettings.SpellWindowState.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public bool ShowRandomRolls
        {
            get => toolSettings.ShowRandomRolls;
            set
            {
                toolSettings.ShowRandomRolls = value;
                OnPropertyChanged();
            }
        }

        public double TriggerWindowOpacity
        {
            get => toolSettings.SpellWindowState.Opacity ?? 1.0;
            set
            {
                toolSettings.SpellWindowState.Opacity = value;
                ((App)System.Windows.Application.Current).UpdateBackgroundOpacity("MyWindowStyleTrigger", value);
                OnPropertyChanged();
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

        private string _SelectedVoice = string.Empty;
        public string SelectedVoice
        {
            get => _SelectedVoice;
            set
            {
                _SelectedVoice = value;
                toolSettings.SelectedVoice = value;
                OnPropertyChanged();
            }
        }

        private string _EqLogPath = string.Empty;
        public string EqLogPath
        {
            get => _EqLogPath;
            set
            {
                _EqLogPath = value;
                OnPropertyChanged();
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
        public ObservableCollection<string> InstalledVoices { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<BoolStringClass> SelectedPlayerClasses { get; set; } = new ObservableCollection<BoolStringClass>();
        public List<MapLocationSharing> LocationShareOptions => Enum.GetValues(typeof(MapLocationSharing)).Cast<MapLocationSharing>().ToList();
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

        public ObservableCollection<string> Zones { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<int> FontSizes { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int> Levels { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int?> TrackSkills { get; set; } = new ObservableCollection<int?>();

        public void Update()
        {
            _ = ActivePlayer.Update();
            OnPropertyChanged(nameof(ActivePlayer));
            OnPropertyChanged(nameof(HasCharName));
            OnPropertyChanged(nameof(HasNoCharName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
