using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(P99WikiByNameId)), Index(nameof(Name)), Index(nameof(Name), nameof(ZoneName))]
    public class P99WikiByName
    {
        public int P99WikiByNameId { get; set; }

        [MaxLength(48)]
        public string? ZoneName { get; set; }

        [MaxLength(64), Required]
        public string Name { get; set; } = string.Empty;

        public string Data { get; set; }
    }
}