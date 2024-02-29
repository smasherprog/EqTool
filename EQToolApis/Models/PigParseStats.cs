using EQToolShared.Enums;

namespace EQToolApis.Models
{
    public class PigParseZoneStat
    {
        public string Zone { get; set; }
        public int Count { get; set; }
    }
    public class PigParseStats
    {
        public Servers Server { get; set; }
        public int PigParsePlayerCount { get; set; }
        public int PigParseUniquePlayerCount { get; set; }
        public List<PigParseZoneStat> zoneStats { get; set; } = new List<PigParseZoneStat>();
    }
}
