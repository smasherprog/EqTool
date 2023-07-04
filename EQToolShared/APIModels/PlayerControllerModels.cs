using EQToolShared.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EQToolShared.APIModels.PlayerControllerModels
{
    public class PlayerRequest
    {
        [Required]
        public List<string> Players { get; set; } = new List<string>();
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }
    public class Player
    {
        [MaxLength(24), MinLength(3), Required]
        public string Name { get; set; }
        [MaxLength(48), MinLength(3)]
        public string GuildName { get; set; }
        public Servers Server { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
        public int? Level { get; set; }
    }

    public class PlayerUpdateRequest
    {
        [Required]
        public List<Player> Players { get; set; } = new List<Player>();
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }
}
