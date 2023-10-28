using EQToolShared.Enums;

namespace EQToolShared.Map
{
    public enum MapLocationSharing
    {
        DoNotShare,
        GuildOnly,
        Everyone
    }

    public class SignalrPlayer
    {
        public string Name { get; set; }
        public string GuildName { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
        public MapLocationSharing MapLocationSharing { get; set; }
        public Servers Server { get; set; }
        public string Zone { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string GroupName => $"{Server}_{Zone}";
    }
}
