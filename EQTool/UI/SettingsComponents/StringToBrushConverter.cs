using EQTool.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EQTool.UI.SettingsComponents
{
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
