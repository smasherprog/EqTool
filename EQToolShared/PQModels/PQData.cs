using System;
using System.Collections.Generic;

namespace EQToolShared.PQModels
{
    [Serializable]
    public class PQData
    {
        public List<item> Items { get; set; }
        public List<lootdrop_entry> lootdrop_Entries { get; set; }
        public List<loottable_entry> loottable_Entries { get; set; }
        public List<npc_type> npc_Types { get; set; }
    }
}
