using EQTool.Services;

namespace EQTool.Models
{
    public class EQMapColor
    {
        public System.Windows.Media.Color DarkColor { get; set; }
        public System.Windows.Media.Color LightColor { get; set; }
        public static EQMapColor GetThemedColors(System.Windows.Media.Color color)
        {
            System.Windows.Media.Color GetDarkThemeTransformedColor(System.Windows.Media.Color c)
            {
                var lightness = c.GetBrightness();
                return lightness < 0.1 ? System.Windows.Media.Color.FromRgb(255, 255, 255) : lightness < .5 ? c.ChangeColorBrightness(.75f) : c;
            }

            return new EQMapColor
            {
                DarkColor = GetDarkThemeTransformedColor(color),
                LightColor = color
            };
        }
    }

}
