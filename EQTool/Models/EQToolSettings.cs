using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.Models
{
    public class WindowState
    {
        public Rect? WindowRect { get; set; }
        public System.Windows.WindowState State { get; set; }
        public bool IsLocked { get; set; }
        public bool Closed { get; set; }
        public bool AlwaysOnTop { get; set; }

        private double _Opacity = 1.0;
        public double? Opacity
        {
            get => _Opacity;
            set => _Opacity = value ?? 1.0;
        }
    }

    public class EQToolSettings : INotifyPropertyChanged
    {
        public string DefaultEqDirectory { get; set; }
        public string EqLogDirectory { get; set; }
        public string SelectedVoice { get; set; }
        private int _FontSize = 12;
        public int? FontSize
        {
            get => _FontSize;
            set
            {
                _FontSize = value ?? 12;
                _FontSize = _FontSize < 12 ? 12 : _FontSize;
            }
        }

        private WindowState _OverlayWindowState;
        public WindowState OverlayWindowState
        {
            get
            {
                if (_OverlayWindowState == null)
                {
                    _OverlayWindowState = new WindowState();
                }
                return _OverlayWindowState;
            }
            set => _OverlayWindowState = value ?? new WindowState();
        }

        private WindowState _SpellWindowState;
        public WindowState SpellWindowState
        {
            get
            {
                if (_SpellWindowState == null)
                {
                    _SpellWindowState = new WindowState();
                }
                return _SpellWindowState;
            }
            set => _SpellWindowState = value ?? new WindowState();
        }

        private WindowState _DpsWindowState;
        public WindowState DpsWindowState
        {
            get
            {
                if (_DpsWindowState == null)
                {
                    _DpsWindowState = new WindowState();
                }
                return _DpsWindowState;
            }
            set => _DpsWindowState = value ?? new WindowState();
        }

        private WindowState _ConsoleWindowState;
        public WindowState ConsoleWindowState
        {
            get
            {
                if (_ConsoleWindowState == null)
                {
                    _ConsoleWindowState = new WindowState();
                }
                return _ConsoleWindowState;
            }
            set => _ConsoleWindowState = value ?? new WindowState();
        }

        private WindowState _MapWindowState;
        public WindowState MapWindowState
        {
            get
            {
                if (_MapWindowState == null)
                {
                    _MapWindowState = new WindowState();
                }
                return _MapWindowState;
            }
            set => _MapWindowState = value ?? new WindowState();
        }

        private WindowState _MobWindowState;
        public WindowState MobWindowState
        {
            get
            {
                if (_MobWindowState == null)
                {
                    _MobWindowState = new WindowState();
                }
                return _MobWindowState;
            }
            set => _MobWindowState = value ?? new WindowState();
        }

        private WindowState _SettingsWindowState;
        public WindowState SettingsWindowState
        {
            get
            {
                if (_SettingsWindowState == null)
                {
                    _SettingsWindowState = new WindowState();
                }
                return _SettingsWindowState;
            }
            set => _SettingsWindowState = value ?? new WindowState();
        }

        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();
        public bool YouOnlySpells { get; set; }
        public bool ShowRandomRolls { get; set; }

        private bool? _ShowRing8RollTime { get; set; } = true;
        public bool? ShowRing8RollTime
        {
            get => _ShowRing8RollTime ?? true;
            set { if (value == null) { _ShowRing8RollTime = true; } else { _ShowRing8RollTime = value; } }
        }

        private bool? _ShowScoutRollTime { get; set; } = true;
        public bool? ShowScoutRollTime
        {
            get => _ShowScoutRollTime ?? true;
            set { if (value == null) { _ShowScoutRollTime = true; } else { _ShowScoutRollTime = value; } }
        }

        private bool? _RaidModeDetection;
        public bool? RaidModeDetection
        {
            get => _RaidModeDetection ?? true;
            set { if (value == null) { _RaidModeDetection = true; } else { _RaidModeDetection = value; } }
        }
        public bool LoginMiddleMand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
