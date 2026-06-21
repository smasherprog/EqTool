using EQTool.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EQTool.UI.SettingsComponents
{
    // Converts a stored color name (e.g. "Red") into a brush for the color-picker
    // swatch previews in the trigger editor.
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TriggerColors.ToBrush(value as string, Brushes.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
