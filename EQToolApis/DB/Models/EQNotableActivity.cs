using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(EQNotableActivityId)), Index(nameof(ActivityTime)), Index(nameof(Server), nameof(IsDeath), nameof(ActivityTime))]
    public class EQNotableActivity
    {
        public int EQNotableActivityId { get; set; }

        public Servers Server { get; set; }

        public double? LocX { get; set; }

        public double? LocY { get; set; }

        public bool IsDeath { get; set; }

        public DateTimeOffset ActivityTime { get; set; }

        public int EQNotableNPCId { get; set; }

        public EQNotableNPC EQNotableNPC { get; set; }
    }
}