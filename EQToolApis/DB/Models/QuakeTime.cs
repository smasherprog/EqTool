using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(QuakeTimeId)), Index(nameof(DateTime))]
    public class QuakeTime
    {
        public int QuakeTimeId { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public Servers Server { get; set; }
    }
}