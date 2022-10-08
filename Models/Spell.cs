namespace EqTool.Models
{
    public class Spell
    {

        public int id; // not used 
        public string name; // Name of the spell 
        public string you_cast; // Message when you cast 
        public string other_casts; // Message when other casts 
        public string cast_on_you; // Message when spell is cast on you  
        public string cast_on_other; // Message when spell is cast on someone else 
        public string spell_fades; // Spell fades
        public int buffdurationformula;
        public int buffduration;
    }
}
