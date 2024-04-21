using System;

namespace EQToolShared.PQModels
{
    [Serializable]
    public class loottable_entry
    {
        public int oottable_id { get; set; }
        public int ootdrop_id { get; set; }
        public int ultiplier { get; set; }
        public int robability { get; set; }
        public int roplimit { get; set; }
        public int indrop { get; set; }
        public int multiplier_min { get; set; }
    }
}
