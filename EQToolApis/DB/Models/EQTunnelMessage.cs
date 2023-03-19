using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQTunnelMessageId)), Index(nameof(DiscordMessageId)), Index(nameof(Server)), Index(nameof(AuctionType))]
    public class EQTunnelMessage
    {
        public long EQTunnelMessageId { get; set; }

        public long DiscordMessageId { get; set; }

        public int EQAuctionPlayerId { get; set; }

        public EQAuctionPlayer EQAuctionPlayer { get; set; }

        public Servers Server { get; set; }

        public AuctionType AuctionType { get; set; }

        public DateTimeOffset TunnelTimestamp { get; set; }

        public ICollection<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }
    }
}