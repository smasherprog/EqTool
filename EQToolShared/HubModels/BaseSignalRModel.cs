using EQToolShared.Enums;
using EQToolShared.Map;

namespace EQToolShared.HubModels
{
    public abstract class BaseSignalRModel
    {
        public string GuildName { get; set; } 
        public MapLocationSharing MapLocationSharing { get; set; }
        public Servers Server { get; set; } 
        public string Zone { get; set; }
        public virtual string GroupName
        {
            get
            {
                if (MapLocationSharing == MapLocationSharing.GuildOnly)
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
