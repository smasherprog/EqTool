using EQToolShared.Enums;
using EQToolShared.Map;

namespace EQToolShared.HubModels
{
    public abstract class BaseSignalRModel
    {
        public string GuildName { get; set; } 
        public MapLocationSharing Sharing { get; set; }
        public Servers Server { get; set; } 
        public string Zone { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }

        public string GroupName
        {
            get
            {
                if (Sharing == MapLocationSharing.GuildOnly)
                {
                    if (!string.IsNullOrWhiteSpace(GuildName))
                    { 
                        return $"{Server}_{Zone}_{GuildName}";
                    }
                }
                return $"{Server}_{Zone}";
            }
        }
    }
}
