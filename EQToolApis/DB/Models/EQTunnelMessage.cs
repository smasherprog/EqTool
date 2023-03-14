using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class EQTunnelMessage
    {
        [PrimaryKey]
        public long EQTunnelMessageId { get; set; }

        [MaxLength(32)]
        public string AuctionPerson { get; set; }

        public Servers Server { get; set; }

        public AuctionType AuctionType { get; set; }

        public DateTimeOffset TunnelTimestamp { get; set; }

        public ICollection<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }
    }
}