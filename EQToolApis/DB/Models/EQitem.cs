using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQitemId))]
    public class EQitem
    {
        public int EQitemId { get; set; }

        [MaxLength(64)]
        public string ItemName { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        public int TotalAuctionCount { get; set; }

        public int TotalAuctionAverage { get; set; }

        public int TotalLast30DaysCount { get; set; }

        public int TotalLast30DaysAverage { get; set; }

        public int TotalLast60DaysCount { get; set; }

        public int TotalLast60DaysAverage { get; set; }

        public int TotalLast90DaysCount { get; set; }

        public int TotalLast90DaysAverage { get; set; }

        public int TotalLast6MonthsCount { get; set; }

        public int TotalLast6MonthsAverage { get; set; }

        public int TotalLastYearCount { get; set; }

        public int TotalLastYearAverage { get; set; }

        public ICollection<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }
    }
}