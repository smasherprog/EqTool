using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace EQTool.Models
{
    // The full set of named colors offered in the trigger editor color pickers,
    // pulled from the WPF System.Windows.Media.Brushes palette, plus a helper to
    // turn a stored color name into a brush at runtime.
    public static class TriggerColors
    {
        public static readonly List<string> Names = typeof(Brushes)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(SolidColorBrush))
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToList();

        public static SolidColorBrush ToBrush(string name, SolidColorBrush fallback)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return fallback;
            }
            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(name);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
