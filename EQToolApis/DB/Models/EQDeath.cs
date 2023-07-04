using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQDeathId))]
    public class EQDeath
    {
        public int EQDeathId { get; set; }

        public Servers Server { get; set; }

        public int EQZoneId { get; set; }

        public EQZone EQZone { get; set; }

        [MaxLength(64), Required]
        public string Name { get; set; } = string.Empty;

        public double? LocX { get; set; }

        public double? LocY { get; set; }

        public DateTimeOffset EQDeathTime { get; set; }
    }
}