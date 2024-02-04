using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;
using System.Collections.Generic;

namespace EQToolShared.Map
{
    public enum MapLocationSharing
    {
        Everyone,
        GuildOnly
    }

    public class NParseStateData
    {
        public Dictionary<string, Dictionary<string, NParseBaseLocation>> locations { get; set; }
    }
    public class NParseBaseLocation
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public string timestamp { get { return _timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff"); } set { } }
        private DateTime _timestamp = DateTime.Now;
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

    public class SignalrPlayer
    {
        public string Name { get; set; }
        public string GuildName { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public double? TrackingDistance { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
        public MapLocationSharing MapLocationSharing { get; set; }
        public Servers Server { get; set; }
        public string Zone { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string GroupName
        {
            get
            {
                if (this.MapLocationSharing == MapLocationSharing.GuildOnly)
                {
                    if (string.IsNullOrWhiteSpace(this.GuildName))
                    {
                        return $"{Server}_{Zone}_{this.Name}";
                    }
                    return $"{Server}_{Zone}_{this.GuildName}";
                }
                return $"{Server}_{Zone}";
            }
        }
    }
    public class TriggerEvent : CustomTimer
    {
        public string GuildName { get; set; }
        public MapLocationSharing MapLocationSharing { get; set; }
        public Servers Server { get; set; }
        public string Zone { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string GroupName
        {
            get
            {
                if (this.MapLocationSharing == MapLocationSharing.GuildOnly)
                {
                    if (string.IsNullOrWhiteSpace(this.GuildName))
                    {
                        return $"{Server}_{Zone}";
                    }
                    return $"{Server}_{Zone}_{this.GuildName}";
                }
                return $"{Server}_{Zone}";
            }
        }
    }
}