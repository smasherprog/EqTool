using System;

namespace EQToolShared.PQModels
{
    [Serializable]
    public class lootdrop_entry
    {
        public int lootdrop_id { get; set; }
        public int item_id { get; set; }
        public int item_charges { get; set; }
        public int equip_item { get; set; }
        public float chance { get; set; }
        public int minlevel { get; set; }
        public int maxlevel { get; set; }
        public int multiplier { get; set; }
        public float disabled_chance { get; set; }
    }
}
