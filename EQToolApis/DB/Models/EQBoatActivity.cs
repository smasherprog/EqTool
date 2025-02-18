using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQBoatActivityId)), Index(nameof(Server), nameof(Zone), nameof(LastSeen)), Index(nameof(Server), nameof(Zone))]
    public class EQBoatActivity
    {
        public int EQBoatActivityId { get; set; }
        [MaxLength(48), Required]
        public string Zone { get; set; } = string.Empty;
        public DateTimeOffset LastSeen { get; set; }
        public Servers Server { get; set; }
    }
}