using EQTool.ViewModels;
using EQToolShared.Map;
using System;

namespace EQTool.Services
{
    public class EnrageParser
    {
        public class EnrageEvent
        {
            public string NpcName { get; set; }
        }

        private readonly ActivePlayer activePlayer;

        public EnrageParser(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public EnrageEvent EnrageCheck(string line)
        {
            if (line.EndsWith(" has become ENRAGED.", System.StringComparison.OrdinalIgnoreCase))
            {
                var npcname = line.Replace(" has become ENRAGED.", string.Empty).Trim();
                if (ZoneParser.ZoneInfoMap.TryGetValue(this.activePlayer.Player.Zone, out var zone))
                {
                    foreach (var item in zone.NotableNPCs)
                    {
                        if (string.Equals(item, npcname, StringComparison.OrdinalIgnoreCase))
                        {
                            return new EnrageEvent { NpcName = npcname };
                        }
                    }
                }
            }

            return null;
        }
    }
}
