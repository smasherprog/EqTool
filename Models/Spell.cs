
using System.Windows;

namespace EQTool.Models
{
    public class Spell : SpellBase
    {
        public const string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";
        public const string YouBeginCasting = "you begin casting ";

        public SpellIcon SpellIcon { get; set; }
        public bool HasSpellIcon => SpellIcon != null;
        public Int32Rect Rect { get; set; }
    }
}
