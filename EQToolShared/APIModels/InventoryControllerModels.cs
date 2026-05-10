using EQToolShared.Enums;
using System.Collections.Generic;

namespace EQToolShared.APIModels.InventoryControllerModels
{
    public class InventoryItemModel
    {
        public InventoryLocation Location { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Slots { get; set; }
    }

    public class InventoryUploadRequest
    {
        public string CharacterName { get; set; } = string.Empty;
        public List<InventoryItemModel> Items { get; set; } = new List<InventoryItemModel>();
    }
}
