using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class CharacterInventory
    {
        public int CharacterInventoryId { get; set; }
        public int DiscordUserId { get; set; }
        [MaxLength(64), Required]
        public string CharacterName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public DiscordUser DiscordUser { get; set; } = null!;
        public List<CharacterInventoryItem> Items { get; set; } = new List<CharacterInventoryItem>();
    }
}
