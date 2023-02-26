using System;

namespace EQTool.Models
{
    public class DPSParseMatch
    {
        public string TargetName { get; set; }

        public string SourceName { get; set; }

        public int DamageDone { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
