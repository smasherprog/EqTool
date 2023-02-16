namespace EQTool.Services
{
    public static class SystemDragingColor
    {
        public static float GetBrightness(this System.Windows.Media.Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetBrightness();
        }

        //correctionFactor -1 to 1
        public static System.Windows.Media.Color ChangeColorBrightness(this System.Windows.Media.Color color, float correctionFactor)
        {
            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = ((255 - red) * correctionFactor) + red;
                green = ((255 - green) * correctionFactor) + green;
                blue = ((255 - blue) * correctionFactor) + blue;
            }

            return System.Windows.Media.Color.FromRgb((byte)red, (byte)green, (byte)blue);
        }
    }
}
