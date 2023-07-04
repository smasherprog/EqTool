using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQNotableNPCId))]
    public class EQNotableNPC
    {
        public int EQNotableNPCId { get; set; }

        public int EQZoneId { get; set; }

        public EQZone EQZone { get; set; }

        [MaxLength(64), Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<EQNotableActivity> EQNotableActivities { get; set; }
    }
}