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

    public class SignalrPlayer: BaseSignalRModel
    {
        public string Name { get; set; } 
        public double? TrackingDistance { get; set; } 
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; } 
    }
}