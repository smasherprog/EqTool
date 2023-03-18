using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQitemId))]
    public class EQitem
    {
        public int EQitemId { get; set; }

        [MaxLength(64)]
        public string ItemName { get; set; }
    }
}