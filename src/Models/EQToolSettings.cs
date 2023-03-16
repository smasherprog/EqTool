using System.Collections.Generic;
using System.Windows;

namespace EQTool.Models
{
    public class WindowState
    {
        public Rect? WindowRect { get; set; }
        public System.Windows.WindowState State { get; set; }
        public bool Closed { get; set; }
    }

    public class EQToolSettings
    {
        public string DefaultEqDirectory { get; set; }
        public string EqLogDirectory { get; set; }

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

        public Themes Theme { get; set; } = Themes.Light;

        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();

        public bool BestGuessSpells { get; set; }

        public bool YouOnlySpells { get; set; }
    }
}
