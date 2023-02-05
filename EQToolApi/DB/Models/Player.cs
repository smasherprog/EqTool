using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EQToolApi.DB.Models
{
    public class Player
    {
        [Column(Order = 0), Key, MaxLength(24), MinLength(3)]
        public string Name { get; set; }
        [Column(Order = 1), Key]
        public Servers Server { get; set; }
        public PlayerClasses PlayerClass { get; set; }
        public byte Level { get; set; }
    }
}