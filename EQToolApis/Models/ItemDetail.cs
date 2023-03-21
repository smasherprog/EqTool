using EQToolApis.DB;

namespace EQToolApis.Models
{
    public class ItemAuctionDetail
    {
        public AuctionType AuctionType { get; set; }

        public string PlayerName { get; set; }

        public int? AuctionPrice { get; set; }

        public DateTimeOffset TunnelTimestamp { get; set; }
    }

    public class ItemDetail
    {
        public List<ItemAuctionDetail> Items { get; set; }

        public string? ItemName { get; set; }
    }
}
