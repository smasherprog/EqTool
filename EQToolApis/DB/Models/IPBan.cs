using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(IpAddress))]
    public class IPBan
    {
        [MaxLength(24)]
        public string? IpAddress { get; set; }
    }
}