using EQTool.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace EQtoolsTests
{
    [TestClass]
    public class TriggerColorsTests
    {
        // The near-black background (ARGB 200,10,10,10) the overlay paints bars/text on. Relative
        // luminance of pure black is 0, so this is effectively black for contrast purposes.
        private static double RelativeLuminance(MediaColor c)
        {
            double Channel(double v)
            {
                v /= 255.0;
                return v <= 0.03928 ? v / 12.92 : System.Math.Pow((v + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
        }

        private static MediaColor ColorOf(string name) => ((SolidColorBrush)new BrushConverter().ConvertFromString(name)).Color;

        [TestMethod]
        public void EveryPaletteColorIsAValidBrush()
        {
            foreach (var name in TriggerColors.Names)
            {
                var brush = TriggerColors.ToBrush(name, null);
                Assert.IsNotNull(brush, $"{name} did not resolve to a brush.");
            }
        }

        [TestMethod]
        public void EveryPaletteColorIsReadableOnADarkBackground()
        {
            // Contrast vs black is (L + 0.05) / 0.05. Require >= 3:1 (WCAG large-text / UI), which
            // means a relative luminance of at least 0.10 — enough to keep every option visible on
            // the overlay's near-black background.
            var failures = TriggerColors.Names
                .Where(n => RelativeLuminance(ColorOf(n)) < 0.10)
                .ToList();
            Assert.AreEqual(0, failures.Count,
                "These palette colors are too dark to read on the overlay: " + string.Join(", ", failures));
        }

        [TestMethod]
        public void PaletteColorsAreNotTooCloseToEachOther()
        {
            // Regression guard against adding near-duplicate entries (two pastels, Yellow next to
            // Gold, etc.). Squared euclidean distance in RGB; the closest legitimate pair in the
            // curated palette is Gold/Orange at 50, so require at least 45 between any two colors.
            const double MinDistanceSquared = 45.0 * 45.0;
            var colors = TriggerColors.Names.Select(n => new { n, c = ColorOf(n) }).ToList();
            var tooClose = new List<string>();
            for (var i = 0; i < colors.Count; i++)
            {
                for (var j = i + 1; j < colors.Count; j++)
                {
                    var a = colors[i].c;
                    var b = colors[j].c;
                    double dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
                    var distSq = (dr * dr) + (dg * dg) + (db * db);
                    if (distSq < MinDistanceSquared)
                    {
                        tooClose.Add($"{colors[i].n}/{colors[j].n}");
                    }
                }
            }
            Assert.AreEqual(0, tooClose.Count, "These palette colors are too similar: " + string.Join(", ", tooClose));
        }
    }
}
