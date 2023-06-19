using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public enum APIAction : short
    {
        PlayerUpdate,
        PlayerAdded,
        DeathEvent
    }

    [PrimaryKey(nameof(APILogId))]
    public class APILog
    {
        public int APILogId { get; set; }

        [MaxLength(24)]
        public string? IpAddress { get; set; }

        public APIAction APIAction { get; set; }

        public string LogMessage { get; set; }
    }
}