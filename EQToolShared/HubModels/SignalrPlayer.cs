using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;

namespace EQToolShared.Map
{
    public enum MapLocationSharing
    {
        Everyone,
        GuildOnly
    }

    public class SignalrPlayerV2: BaseSignalRModel
    {
        public string Name { get; set; } 
        public double? TrackingDistance { get; set; }  
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
}