namespace EQToolApis.Models
{
    public class RangeTimeNPCDateTime
    {
        public DateTimeOffset BegWindow { get; set; }
        public DateTimeOffset EndWindow { get; set; }
    }

    public class TODModel
    {
        public string Name { get; set; }

        public DateTimeOffset? EventTime { get; set; }
        public List<DateTimeOffset> FixedTimeNPCDateTimes { get; set; }
        public List<RangeTimeNPCDateTime> RangeTimeNPCDateTime { get; set; }
    }
}
