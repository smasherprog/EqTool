using System;

namespace EQTool.Models
{
    [Serializable]
    public struct Colour
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public Colour(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Colour(System.Windows.Media.Color color) : this(color.A, color.R, color.G, color.B)
        {
        }

        public static implicit operator Colour(System.Windows.Media.Color color)
        {
            return new Colour(color);
        }

        public static implicit operator System.Windows.Media.Color(Colour colour)
        {
            return System.Windows.Media.Color.FromArgb(colour.A, colour.R, colour.G, colour.B);
        }
    }

    [Serializable]
    public class EQMapColor
    {
        public Colour DarkColor { get; set; }
        public Colour LightColor { get; set; }
        public Colour OriginalColor { get; set; }
        public static EQMapColor GetThemedColors(System.Windows.Media.Color color)
        {
            System.Windows.Media.Color GetDarkThemeTransformedColor(System.Windows.Media.Color c)
            {
                var ctrrt = new HslColor(c);
                return ctrrt.l < 0.1 ? System.Windows.Media.Color.FromRgb(255, 255, 255) : ctrrt.Lighten(1).ToRgb();
            }
            System.Windows.Media.Color GetLightThemeTransformedColor(System.Windows.Media.Color c)
            {
                var ctrrt = new HslColor(c);
                return ctrrt.l > 0.9 ? System.Windows.Media.Color.FromRgb(0, 0, 0) : c;
            }
            return new EQMapColor
            {
                DarkColor = GetDarkThemeTransformedColor(color),
                LightColor = GetLightThemeTransformedColor(color),
                OriginalColor = color
            };
        }
    }

    public class HslColor
    {
        public readonly double h, s, l, a;

        public HslColor(double h, double s, double l, double a)
        {
            this.h = h;
            this.s = s;
            this.l = l;
            this.a = a;
        }

        public HslColor(System.Windows.Media.Color rgb)
        {
            RgbToHls(rgb.R, rgb.G, rgb.B, out h, out l, out s);
            a = rgb.A / 255.0;
        }

        public System.Windows.Media.Color ToRgb()
        {
            HlsToRgb(h, l, s, out var r, out var g, out var b);
            return System.Windows.Media.Color.FromArgb((byte)(a * 255.0), (byte)r, (byte)g, (byte)b);
        }

        public HslColor Lighten(double amount)
        {
            return new HslColor(h, s, Clamp(l * amount, 0, 1), a);
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min ? min : value > max ? max : value;
        }

        // Convert an RGB value into an HLS value.
        private static void RgbToHls(int r, int g, int b, out double h, out double l, out double s)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            var double_r = r / 255.0;
            var double_g = g / 255.0;
            var double_b = b / 255.0;

            // Get the maximum and minimum RGB components.
            var max = double_r;
            if (max < double_g)
            {
                max = double_g;
            }

            if (max < double_b)
            {
                max = double_b;
            }

            var min = double_r;
            if (min > double_g)
            {
                min = double_g;
            }

            if (min > double_b)
            {
                min = double_b;
            }

            var diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                s = l <= 0.5 ? diff / (max + min) : diff / (2 - max - min);

                var r_dist = (max - double_r) / diff;
                var g_dist = (max - double_g) / diff;
                var b_dist = (max - double_b) / diff;

                h = double_r == max ? b_dist - g_dist : double_g == max ? 2 + r_dist - b_dist : 4 + g_dist - r_dist;

                h *= 60;
                if (h < 0)
                {
                    h += 360;
                }
            }
        }

        // Convert an HLS value into an RGB value.
        private static void HlsToRgb(double h, double l, double s,
            out int r, out int g, out int b)
        {
            var p2 = l <= 0.5 ? l * (1 + s) : l + s - (l * s);
            var p1 = (2 * l) - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360)
            {
                hue -= 360;
            }
            else if (hue < 0)
            {
                hue += 360;
            }

            return hue < 60 ? q1 + ((q2 - q1) * hue / 60) : hue < 180 ? q2 : hue < 240 ? q1 + ((q2 - q1) * (240 - hue) / 60) : q1;
        }
    }

    public static class MathHelper
    {
        public static double ChangeRange(double x, double fromleft, double fromright, double toleft, double toright)
        {
            return ((x - fromleft) * (toright - toleft) / (fromright - fromleft)) + toleft;
        }
    }
}
