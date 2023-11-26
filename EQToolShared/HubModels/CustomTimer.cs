using EQToolShared.Enums;

namespace EQToolShared.HubModels
{
    public class CustomTimer
    {
        public string Name { get; set; }
        public int DurationInSeconds { get; set; }
    }
    public class SignalrCustomTimer : CustomTimer
    {
        public Servers Server { get; set; }
    }
}
