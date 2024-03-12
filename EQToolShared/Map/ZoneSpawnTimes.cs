using System;
using System.Collections.Generic;
using System.Linq;

namespace EQToolShared.Map
{
    public static class ZoneSpawnTimes
    {
#if QUARM
        public static bool isProjectQ = true;
#else
        public static bool isProjectQ = false;
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

        private const double RespawnReductionDungeonLowerBoundMin = 300;
        private const double RespawnReductionDungeonHigherBoundMin = 900;

        private const double RespawnReductionDungeonLowerBoundMax = 899;
        private const double RespawnReductionDungeonHigherBoundMax = 2400;

        private const double RespawnReductionLowerBoundMin = 60;
        private const double RespawnReductionHigherBoundMin = 100;

        private const double RespawnReductionDungeonLowerBound = 300;
        private const double RespawnReductionDungeonHigherBound = 500;

        private const double RespawnReductionHigherBoundMax = 60;
        private const double RespawnReductionHigherBound = 60;

        private const double RespawnReductionLowerBoundMax = 400;
        private const double RespawnReductionLowerBound = 12;

        public static TimeSpan GetSpawnTime(string npcName, string zone)
        {
            var ret = _GetSpawnTime(npcName, zone);
            if (isProjectQ)
            {
                if (Dungeons.Contains(zone))
                {
                    if (ret.TotalSeconds >= RespawnReductionDungeonHigherBoundMin && ret.TotalSeconds <= RespawnReductionDungeonHigherBoundMax)
                    {
                        return TimeSpan.FromSeconds(RespawnReductionDungeonHigherBound);
                    }
                    else if (ret.TotalSeconds >= RespawnReductionDungeonLowerBoundMin && ret.TotalSeconds <= RespawnReductionDungeonLowerBoundMax)
                    {
                        return TimeSpan.FromSeconds(RespawnReductionDungeonLowerBound);
                    }
                }
                else
                {
                    //ignore low levels now
                    //if (ret.TotalSeconds >= RespawnReductionHigherBoundMin && ret.TotalSeconds <= RespawnReductionHigherBoundMax)
                    //{
                    //    return TimeSpan.FromSeconds(RespawnReductionHigherBound);
                    //}
                    //else if (ret.TotalSeconds >= RespawnReductionLowerBoundMin && ret.TotalSeconds <= RespawnReductionLowerBoundMax)
                    //{
                    //    return TimeSpan.FromSeconds(RespawnReductionLowerBound);
                    //}
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
