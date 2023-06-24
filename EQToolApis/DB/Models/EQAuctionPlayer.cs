using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQAuctionPlayerId)), Index(nameof(Server))]
    public class EQAuctionPlayer
    {
        public int EQAuctionPlayerId { get; set; }

        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        public Servers Server { get; set; }
    }
}