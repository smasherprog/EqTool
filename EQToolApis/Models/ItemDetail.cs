using EQToolApis.DB;

namespace EQToolApis.Models
{
    public class ItemAuctionDetail
    {
        /// <summary>
        /// Auction Type
        /// </summary>
        public AuctionType u { get; set; }
        /// <summary>
        /// Player Id. Use Players collection to reference the name
        /// </summary>
        public int i { get; set; }
        /// <summary>
        /// Auction Price. Will be null if no price is listed
        /// </summary>
        public int? p { get; set; }
        /// <summary>
        /// The Date Time the auction occurred.
        /// </summary>
        public DateTimeOffset t { get; set; }
    }

    public class ItemDetail
    {
        public List<ItemAuctionDetail> Items { get; set; }

        public string? ItemName { get; set; }
        /// <summary>
        /// This is to reduce on the amount of data sent. All players are listed here for the Items array. 
        /// Use the variable    i    to lookup  a player.
        /// </summary>
        public Dictionary<int, string> Players { get; set; }
    }
}
