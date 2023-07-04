using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQZoneId))]
    public class EQZone
    {
        public int EQZoneId { get; set; }

        [MaxLength(48), Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<EQDeath> EQDeaths { get; set; }
    }
}