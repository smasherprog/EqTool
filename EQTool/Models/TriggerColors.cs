using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Models
{
    // The set of named colors offered in the trigger editor color pickers (bar color and
    // display-text color), plus a helper to turn a stored color name into a brush at runtime.
    //
    // Timer bars and overlay/display text are drawn on a near-black background (the overlay
    // fills with ARGB 200,10,10,10), so the palette is deliberately curated rather than the full
    // ~140-name WPF Brushes set: every entry is bright/medium enough to read against black, and
    // the hues are spread out so no two are easy to confuse (the old list had e.g. Yellow next to
    // White and many near-duplicate pastels and near-black colors that vanished on the overlay).
    // Ordered by hue (with White first) so the picker reads as a rainbow.
    //
    // Any color name still resolves at runtime via ToBrush (it uses BrushConverter, not this list),
    // so triggers saved with an older color keep rendering; they just won't be pre-selected here.
    public static class TriggerColors
    {
        public static readonly List<string> Names = new List<string>
        {
            "White",
            "Red",
            "Orange",
            "Gold",
            "GreenYellow",
            "LimeGreen",
            "SpringGreen",
            "DeepSkyBlue",
            "CornflowerBlue",
            "MediumPurple",
            "Magenta",
            "HotPink",
        };

        // Resolved-brush cache so trigger fires don't allocate a BrushConverter and re-parse
        // the color name every time. Entries are frozen so they are safe to share across
        // threads; a null entry means the name failed to parse (so it isn't retried per fire).
        private static readonly Dictionary<string, SolidColorBrush> brushCache = new Dictionary<string, SolidColorBrush>(System.StringComparer.OrdinalIgnoreCase);

        public static SolidColorBrush ToBrush(string name, SolidColorBrush fallback)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return fallback;
            }
            lock (brushCache)
            {
                if (!brushCache.TryGetValue(name, out var brush))
                {
                    try
                    {
                        brush = (SolidColorBrush)new BrushConverter().ConvertFromString(name);
                        if (brush != null && brush.CanFreeze)
                        {
                            brush.Freeze();
                        }
                    }
                    catch
                    {
                        brush = null;
                    }
                    brushCache[name] = brush;
                }
                return brush ?? fallback;
            }
        }
    }
}
