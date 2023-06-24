using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQTunnelAuctionItemId))]
    public class EQTunnelAuctionItem
    {
        public long EQTunnelAuctionItemId { get; set; }

        public int EQitemId { get; set; }

        public EQitem EQitem { get; set; }

        public Servers Server { get; set; }

        public int? AuctionPrice { get; set; }

        public long EQTunnelMessageId { get; set; }

        public EQTunnelMessage EQTunnelMessage { get; set; }
    }
}