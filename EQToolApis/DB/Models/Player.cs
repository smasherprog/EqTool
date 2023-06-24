using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    [PrimaryKey(nameof(Name), nameof(Server))]
    public class Player
    {
        [MaxLength(24), MinLength(3)]
        public string? Name { get; set; }
        [MaxLength(48), MinLength(3)]
        public string? GuildName { get; set; }
        public Servers Server { get; set; }
        public PlayerClasses? PlayerClass { get; set; }
        public byte? Level { get; set; }
    }
}