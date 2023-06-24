using EQToolShared.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Models
{
    public class ItemsLookups
    {
        [DefaultValue(Servers.Green)]
        public Servers Server { get; set; }

        [Required]
        public List<string>? Itemnames { get; set; }
    }
}
