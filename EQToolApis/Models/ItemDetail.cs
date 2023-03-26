using EQToolApis.DB;

namespace EQToolApis.Models
{
    public class CharJsFormat
    {
        public DateTimeOffset x { get; set; }
        public int y { get; set; }
    }

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

        public List<CharJsFormat> ChartItems => Items.Where(a => a.AuctionPrice.HasValue).Select(a => new CharJsFormat
        {
            x = a.TunnelTimestamp,
            y = a.AuctionPrice.Value
        }).ToList();

        public string? ItemName { get; set; }
    }
}
