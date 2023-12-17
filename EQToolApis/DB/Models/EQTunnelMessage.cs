using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQTunnelMessageId)), Index(nameof(DiscordMessageId)), Index(nameof(Server)), Index(nameof(AuctionType)), Index(nameof(TunnelTimestamp)), Index(nameof(Server), nameof(AuctionType)), Index(nameof(TunnelTimestamp), nameof(AuctionType))]
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

    [PrimaryKey(nameof(EQTunnelMessageId)), Index(nameof(DiscordMessageId)), Index(nameof(Server)), Index(nameof(AuctionType)), Index(nameof(TunnelTimestamp)), Index(nameof(Server), nameof(AuctionType)), Index(nameof(TunnelTimestamp), nameof(AuctionType))]
    public class EQTunnelMessageV2
    {
        public long EQTunnelMessageId { get; set; }

        public long DiscordMessageId { get; set; }

        public int EQAuctionPlayerId { get; set; }

        public EQAuctionPlayerV2 EQAuctionPlayer { get; set; }

        public Servers Server { get; set; }

        public AuctionType AuctionType { get; set; }

        public DateTimeOffset TunnelTimestamp { get; set; }

        public ICollection<EQTunnelAuctionItemV2> EQTunnelAuctionItems { get; set; }
    }
}