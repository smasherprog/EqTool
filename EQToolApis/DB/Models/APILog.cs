using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public enum APIAction : short
    {
        DONTUSE,
        DeathActivity,
        PlayerUpdate,
        PlayerAdded,
        DeathEvent,
        NPCActivity
    }

    [PrimaryKey(nameof(APILogId))]
    public class APILog
    {
        public int APILogId { get; set; }

        [MaxLength(24), Required]
        public string IpAddress { get; set; } = string.Empty;

        public APIAction APIAction { get; set; }

        public string LogMessage { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}