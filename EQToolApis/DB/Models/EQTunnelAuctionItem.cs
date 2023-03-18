using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class EQTunnelAuctionItem
    {
        [Key]
        public long EQTunnelAuctionItemId { get; set; }

        [MaxLength(64)]
        public string ItemName { get; set; }

        public int? AuctionPrice { get; set; }

        public long EQTunnelMessageId { get; set; }

        public EQTunnelMessage EQTunnelMessage { get; set; }
    }
}