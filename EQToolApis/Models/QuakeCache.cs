using EQToolShared.Enums;

namespace EQToolApis.Models
{
    public class QuakeCache
    {
        public DateTimeOffset DateTime { get; set; }
        public Servers Server { get; set; }
    }
}
