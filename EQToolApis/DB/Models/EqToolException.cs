using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(Id))]
    public class EqToolException
    {
        public int Id { get; set; }

        public string? Exception { get; set; }

        [MaxLength(24)]
        public string? Version { get; set; }

        [MaxLength(24)]
        public string? IpAddress { get; set; }

        public DateTime DateCreated { get; set; }

        public EventType? EventType { get; set; }
        public BuildType? BuildType { get; set; }
    }
}