using System.Collections.Generic;
using System.Windows;

namespace EQTool.Models
{
    public class WindowState
    {
        public Rect WindowRect { get; set; }
        public System.Windows.WindowState State { get; set; }
        public bool Closed { get; set; }
    }

    public class EQToolSettings
    {
        public double FontSize { get; set; }

        public string DefaultEqDirectory { get; set; }
         
        public WindowState SpellWindowState { get; set; }

        public WindowState DpsWindowState { get; set; }

        public Themes Theme { get; set; } = Themes.Light;

        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();

        public double GlobalTriggerWindowOpacity { get; set; }

        public double GlobalDPSWindowOpacity { get; set; }

        public bool TriggerWindowTopMost { get; set; }

        public bool BestGuessSpells { get; set; }

        public bool YouOnlySpells { get; set; }
    }
}
