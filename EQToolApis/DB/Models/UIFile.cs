using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EQToolApis.DB.Models
{
    // One EverQuest UI config file backed up per Discord user. These are plain
    // text .ini configs, stored as varchar(max) in Azure SQL. One current version
    // per (user, file name): the unique index enforces the latest-wins upsert.
    [Index(nameof(DiscordUserId), nameof(FileName), nameof(Server), IsUnique = true)]
    public class UIFile
    {
        public int UIFileId { get; set; }
        public int DiscordUserId { get; set; }
        [MaxLength(260), Required]
        public string FileName { get; set; } = string.Empty;
        [MaxLength(64), Required]
        public string PlayerName { get; set; } = string.Empty;
        // Same character/file can exist on multiple servers for one discord user.
        public Servers Server { get; set; } = Servers.Green;
        // Client-supplied last-write time of the file on disk (UTC).
        public DateTime LastModifiedUtc { get; set; }
        // Server receipt time (UTC).
        public DateTime UploadedAtUtc { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Contents { get; set; } = string.Empty;
        public DiscordUser DiscordUser { get; set; } = null!;
    }
}
