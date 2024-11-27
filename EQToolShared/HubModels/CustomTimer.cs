using EQToolShared.Enums;

namespace EQToolShared.HubModels
{
    public class HubCustomTimer
    { 
        public string Name { get; set; } 
        public int DurationInSeconds { get; set; }
        public string SpellNameIcon { get; set; } 
        public string TargetName { get; set; }
    }

    public class SignalrCustomTimer : HubCustomTimer
    {
        public Servers Server { get; set; }
    }
}
