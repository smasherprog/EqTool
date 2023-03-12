using System.ComponentModel.DataAnnotations;

namespace EQToolApis.DB.Models
{
    public class EqToolException
    {
        [Key]
        public int Id { get; set; }

        public string Exception { get; set; }

        [MaxLength(24)]
        public string Version { get; set; }

        public DateTime DateCreated { get; set; }
    }
}