using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQitemId)), Index(nameof(Server)), Index(nameof(ItemName), nameof(Server))]
    public class EQitem
    {
        public int EQitemId { get; set; }

        [MaxLength(64)]
        public string ItemName { get; set; } = string.Empty;

        public Servers Server { get; set; }

        public DateTimeOffset? LastWTBSeen { get; set; }

        public DateTimeOffset? LastWTSSeen { get; set; }

        public int TotalWTSAuctionCount { get; set; }

        public int TotalWTSAuctionAverage { get; set; }

        public int TotalWTSLast30DaysCount { get; set; }

        public int TotalWTSLast30DaysAverage { get; set; }

        public int TotalWTSLast60DaysCount { get; set; }

        public int TotalWTSLast60DaysAverage { get; set; }

        public int TotalWTSLast90DaysCount { get; set; }

        public int TotalWTSLast90DaysAverage { get; set; }

        public int TotalWTSLast6MonthsCount { get; set; }

        public int TotalWTSLast6MonthsAverage { get; set; }

        public int TotalWTSLastYearCount { get; set; }

        public int TotalWTSLastYearAverage { get; set; }

        public int TotalWTBAuctionCount { get; set; }

        public int TotalWTBAuctionAverage { get; set; }

        public int TotalWTBLast30DaysCount { get; set; }

        public int TotalWTBLast30DaysAverage { get; set; }

        public int TotalWTBLast60DaysCount { get; set; }

        public int TotalWTBLast60DaysAverage { get; set; }

        public int TotalWTBLast90DaysCount { get; set; }

        public int TotalWTBLast90DaysAverage { get; set; }

        public int TotalWTBLast6MonthsCount { get; set; }

        public int TotalWTBLast6MonthsAverage { get; set; }

        public int TotalWTBLastYearCount { get; set; }

        public int TotalWTBLastYearAverage { get; set; }

        public ICollection<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }
    }
}