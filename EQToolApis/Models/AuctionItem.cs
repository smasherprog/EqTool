using EQToolApis.DB;
using EQToolShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Models
{
    public class AuctionItem
    {
        /// <summary>
        /// itemid
        /// </summary>
        public int i { get; set; }
        /// <summary>
        /// auction Type
        /// </summary>
        public AuctionType t { get; set; }
        /// <summary>
        /// Item Name
        /// </summary>
        public string n { get; set; } = string.Empty;
        /// <summary>
        /// Last Time seen in tunnel
        /// </summary>
        public DateTimeOffset l { get; set; }
        /// <summary>
        /// Total count of auctions
        /// </summary>
        public int tc { get; set; }
        /// <summary>
        /// Overall average item price
        /// </summary>
        public int ta { get; set; }
        /// <summary>
        /// Last 30 day Auction count
        /// </summary>
        public int t30 { get; set; }
        /// <summary>
        /// Last 30 day running average
        /// </summary>
        public int a30 { get; set; }
        /// <summary>
        /// Last 60 day Auction count
        /// </summary>
        public int t60 { get; set; }
        /// <summary>
        /// Last 60 Day average price
        /// </summary>
        public int a60 { get; set; }
        /// <summary>
        /// Last 90 day Auction count
        /// </summary>
        public int t90 { get; set; }
        /// <summary>
        /// Last 90 day average price
        /// </summary>
        public int a90 { get; set; }
        /// <summary>
        /// Last 6 month Auction count
        /// </summary>
        public int t6m { get; set; }
        /// <summary>
        /// Last 6 Month average price
        /// </summary>
        public int a6m { get; set; }
        /// <summary>
        /// Last 1 year Auction count
        /// </summary>
        public int ty { get; set; }
        /// <summary>
        /// Last 1 year average price
        /// </summary>
        public int ay { get; set; }
    }

    public class Item
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

    }
}
