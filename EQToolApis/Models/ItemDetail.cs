using EQToolApis.DB;

namespace EQToolApis.Models
{
    public class ItemAuctionDetail
    {
        public AuctionType u { get; set; }

        public int i { get; set; }

        public int? p { get; set; }

        public DateTimeOffset t { get; set; }
    }

    public class ItemDetail
    {
        public List<ItemAuctionDetail> Items { get; set; }

        public string? ItemName { get; set; }

        public Dictionary<int, string> Players { get; set; }
    }
}
