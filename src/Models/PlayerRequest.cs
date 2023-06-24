using EQToolShared.Enums;

namespace EQTool.Models
{
    public class PlayerRequest
    {
        public string Name { get; set; }
        public string GuildName { get; set; }
        public Servers Server { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
        public byte? Level { get; set; }
    }
}
