using EQToolShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class CharacterInventoryItem
    {
        public int CharacterInventoryItemId { get; set; }
        public int CharacterInventoryId { get; set; }
        public InventoryLocation Location { get; set; }
        [MaxLength(256), Required]
        public string Name { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Slots { get; set; }
        public CharacterInventory Inventory { get; set; } = null!;
    }
}
