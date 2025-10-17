
using System.Windows;

namespace EQTool.Models
{
    public class Spell : SpellBase
    {
        public SpellIcon SpellIcon { get; set; }
        public bool HasSpellIcon => SpellIcon != null;
        public Int32Rect Rect { get; set; }
        
        public string NameIfSelfCast(string casterName) => SpellType == SpellType.Self ? casterName : string.Empty;
    }
}
