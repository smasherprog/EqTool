using System;

namespace EQTool.Models
{
    [Serializable]
    public class PlayerInfo
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
    }
}
