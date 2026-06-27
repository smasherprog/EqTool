
using System.Windows;

namespace EQTool.Models
{
    public class Spell : SpellBase
    {
        public SpellIcon SpellIcon { get; set; }
        public bool HasSpellIcon => SpellIcon != null;
        public Int32Rect Rect { get; set; }

        // A ready-to-display, frozen crop of the icon sheet. Bind Image.Source to this instead of
        // declaring a CroppedBitmap in XAML: a CroppedBitmap can only be cropped once, so when a
        // single template instance is reused (e.g. a ComboBox's selection box) its image won't
        // update on selection change. Producing a fresh frozen bitmap per item avoids that.
        private System.Windows.Media.ImageSource _croppedIcon;
        [Newtonsoft.Json.JsonIgnore]
        public System.Windows.Media.ImageSource CroppedIcon
        {
            get
            {
                if (_croppedIcon == null && SpellIcon?.Icon != null)
                {
                    var cropped = new System.Windows.Media.Imaging.CroppedBitmap(SpellIcon.Icon, Rect);
                    cropped.Freeze();
                    _croppedIcon = cropped;
                }
                return _croppedIcon;
            }
        }
    }
}
