using System;
using System.Collections.Generic;

namespace EQToolShared.Map
{
    public class NParseStateData
    {
        public Dictionary<string, Dictionary<string, NParseBaseLocation>> locations { get; set; }
    }
    public class NParseBaseLocation
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public string timestamp { get => _timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff"); set { } }
        private readonly DateTime _timestamp = DateTime.Now;
    }
    public class NParseLocation : NParseBaseLocation
    {
        public string zone { get; set; }
        public string player { get; set; }
    }
    public class NParseLocationEvent
    {
        public string group_key { get; set; } = "public";
        public string type { get; set; } = "location";
        public NParseLocation location { get; set; }
    } 
}