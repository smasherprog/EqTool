using System.Collections.Generic;

namespace EQTool.Models
{
    public class EQToolSettings
    {
        public double FontSize { get; set; }

        public string DefaultEqDirectory { get; set; }

        public string Theme { get; set; } = "Light.Blue";

        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();

        public double GlobalTriggerWindowOpacity { get; set; }

        public double GlobalDPSWindowOpacity { get; set; }

        public bool TriggerWindowTopMost { get; set; }

        public bool BestGuessSpells { get; set; }

        public bool YouOnlySpells { get; set; }
    }
}
