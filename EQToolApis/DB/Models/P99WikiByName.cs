using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(P99WikiByNameId))]
    public class P99WikiByName
    {
        public int P99WikiByNameId { get; set; }

        [MaxLength(48)]
        public string ZoneName { get; set; } = string.Empty;

        [MaxLength(64), Required]
        public string Name { get; set; } = string.Empty;

        public string Data { get; set; } = string.Empty;
    }
}