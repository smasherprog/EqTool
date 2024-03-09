using System;
using System.Collections.Generic;
using System.Linq;

namespace EQToolShared.Map
{
    public static class ZoneSpawnTimes
    {
#if QUARM
        private static bool isProjectQ = true;
#else
        private static bool isProjectQ = false;
#endif

        private static List<string> Dungeons = new List<string>()
        {
            "unrest"
            ,"mistmoore"
            ,"najena"
            ,"blackburrow"
            ,"gukbottom"
            ,"guktop"
            ,"highkeep"
            ,"kedge"
            ,"paw"
            ,"permafrost"
            ,"soldunga"
            ,"soldungb"
            ,"cazicthule"
        };

        public static TimeSpan GetSpawnTime(string npcName, string zone)
        {
            var ret = _GetSpawnTime(npcName, zone);
            if (isProjectQ)
            {
                if (Dungeons.Contains(zone))
                {
                    if (ret.TotalSeconds >= 900 && ret.TotalSeconds <= 2400)
                    {
                        return TimeSpan.FromSeconds(400);
                    }
                    else if (ret.TotalSeconds >= 300 && ret.TotalSeconds <= 899)
                    {
                        return TimeSpan.FromSeconds(360);
                    }
                }
                else
                {
                    if (ret.TotalSeconds >= 60 && ret.TotalSeconds <= 300)
                    {
                        return TimeSpan.FromSeconds(60);
                    }
                    else if (ret.TotalSeconds >= 12 && ret.TotalSeconds <= 60)
                    {
                        return TimeSpan.FromSeconds(12);
                    }
                }
            }
            return ret;
        }

        private static TimeSpan _GetSpawnTime(string npcName, string zone)
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
