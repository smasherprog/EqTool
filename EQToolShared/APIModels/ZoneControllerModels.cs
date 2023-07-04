using EQToolShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EQToolShared.APIModels.ZoneControllerModels
{
    public class DeathDataRequest
    {
        [Required]
        public NPCData Death { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }

    public class NPCData
    {
        [MaxLength(64), Required]
        public string Name { get; set; } = string.Empty;
        [MaxLength(64), Required]
        public string Zone { get; set; } = string.Empty;
        public double? LocX { get; set; }
        public double? LocY { get; set; }
    }

    public class NPCActivityRequest
    {
        [Required]
        public NPCData NPCData { get; set; }
        public bool IsDeath { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }
}
