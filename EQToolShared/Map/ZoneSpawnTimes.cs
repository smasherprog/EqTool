using System;
using System.Linq;

namespace EQToolShared.Map
{
    public static class ZoneSpawnTimes
    {
        public static TimeSpan GetSpawnTime(string npcName, string zone)
        {
            if (EQToolShared.Map.ZoneParser.ZoneInfoMap.TryGetValue(zone, out var zoneInfo))
            {
                if (!string.IsNullOrWhiteSpace(npcName))
                {
                    var foundnpc = zoneInfo.NpcSpawnTimes.FirstOrDefault(a => string.Equals(a.Name, npcName, StringComparison.OrdinalIgnoreCase));
                    if (foundnpc != null)
                    {
                        return foundnpc.RespawnTime;
                    }
                    foundnpc = zoneInfo.NpcContainsSpawnTimes.FirstOrDefault(a => npcName.IndexOf(a.Name, StringComparison.OrdinalIgnoreCase) != -1);
                    if (foundnpc != null)
                    {
                        return foundnpc.RespawnTime;
                    }
                }

                return zoneInfo.RespawnTime;
            }

            return new TimeSpan(0, 6, 40);
        }
    }
}
