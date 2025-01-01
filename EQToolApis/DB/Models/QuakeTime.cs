using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(DateTime))]
    public class QuakeTime
    {
        public DateTimeOffset DateTime { get; set; }
        public Servers Server { get; set; }
    }
}