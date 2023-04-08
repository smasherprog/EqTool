using EQToolApis.DB;

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
        public string n { get; set; }
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
}
