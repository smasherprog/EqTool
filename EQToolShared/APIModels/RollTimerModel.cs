using System;

namespace EQToolShared.APIModels
{
    public enum RollTimerType
    {
        Scout = 1,
        Quake = 2
    }

    public class RollTimerModel
    {
        public RollTimerType RollTimerType { get; set; }
        public bool Guess { get; set; } = false;
        public string Name { get; set; }
        public DateTimeOffset DateTime { get; set; }
    }
}
