using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class DiscordUser
    {
        public int DiscordUserId { get; set; }
        [MaxLength(32), Required]
        public string DiscordId { get; set; } = string.Empty;
        [MaxLength(64), Required]
        public string Username { get; set; } = string.Empty;
        [MaxLength(256)]
        public string? Email { get; set; }
        [MaxLength(256)]
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        [MaxLength(64)]
        public string? ApiToken { get; set; }
    }
}
