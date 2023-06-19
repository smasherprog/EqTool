using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQZoneId))]
    public class EQZone
    {
        public int EQZoneId { get; set; }

        [MaxLength(48)]
        public string Name { get; set; }

        public ICollection<EQDeath> EQDeaths { get; set; }
    }
}