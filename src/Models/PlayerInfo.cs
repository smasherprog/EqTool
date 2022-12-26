using System;
using System.Collections.Generic;

namespace EQTool.Models
{
    [Serializable]
    public class PlayerInfo
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Zone { get; set; } 
        public PlayerClasses? PlayerClass { get; set; }
        public List<PlayerClasses> ShowSpellsForClasses { get; set; }
    }
}
