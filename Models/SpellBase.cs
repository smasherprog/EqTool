namespace EQTool.Models
{
    public class SpellBase
    {
        public int id { get; set; } // not used 
        public string name { get; set; } // Name of the spell  
        public string cast_on_you { get; set; } // Message when spell is cast on you  
        public string cast_on_other { get; set; } // Message when spell is cast on someone else 
        public string spell_fades { get; set; } // Spell fades
        public int buffdurationformula { get; set; }
        public int spell_icon { get; set; }
        public int buffduration { get; set; }
    }
}
