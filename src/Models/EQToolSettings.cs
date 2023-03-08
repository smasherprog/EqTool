using System.Collections.Generic;
using System.Windows;

namespace EQTool.Models
{
    public class WindowState
    {
        public Rect? WindowRect { get; set; }
        public System.Windows.WindowState State { get; set; }
        public bool Closed { get; set; }
        public bool TopMost { get; set; }
    }

    public class EQToolSettings
    {
        public string DefaultEqDirectory { get; set; }
        public string EqLogDirectory { get; set; }

        public WindowState SpellWindowState { get; set; }

        public WindowState DpsWindowState { get; set; }

        public WindowState MapWindowState { get; set; }

        public WindowState MobWindowState { get; set; }

        public Themes Theme { get; set; } = Themes.Light;

        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();

        public bool BestGuessSpells { get; set; }

        public bool YouOnlySpells { get; set; }
    }
}
