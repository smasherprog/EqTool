using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWidget.xaml
    /// </summary>
    public partial class MapWidget : UserControl
    {
        private readonly DateTime EndTime;
        private readonly int TotalInitialSeconds;
        public MapWidget(DateTime endtime, double smallFontSize, string name)
        {
            InitializeComponent();
            ClockTextValue.FontSize = smallFontSize;
            EndTime = endtime;
            TotalInitialSeconds = (int)(EndTime - DateTime.Now).TotalSeconds;
            _ = Update();
            Label.Text = name;
            Label.Visibility = string.IsNullOrWhiteSpace(name) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            ClockText.Visibility = string.IsNullOrWhiteSpace(name) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public void SetTheme(Brush labelb, Brush backgroundb)
        {
            ClockText.Foreground = labelb;
            ClockTextValue.Foreground = labelb;
            Background = backgroundb;
        }

        private static System.Windows.Media.Color GetColorFromRedYellowGreenGradient(double percentage)
        {
            var red = (percentage > 50 ? 1 - (2 * (percentage - 50) / 100.0) : 1.0) * 255;
            var green = (percentage > 50 ? 1.0 : 2 * percentage / 100.0) * 200;
            var blue = 0.0;
            var r = System.Windows.Media.Color.FromArgb(200, (byte)red, (byte)green, (byte)blue);
            return r;
        }

        public int Update()
        {
            var timespan = EndTime - DateTime.Now;
            var timespanstring = string.Empty;
            var percentleft = timespan.TotalSeconds / TotalInitialSeconds * 100;
            Background = new SolidColorBrush(GetColorFromRedYellowGreenGradient(percentleft));
            var timepart = (int)timespan.TotalHours;
            if (timepart > 0)
            {
                timespanstring += $"{timepart}:";
            }
            timepart = timespan.Minutes;
            timespanstring += $"{timepart:D2}:";
            timepart = timespan.Seconds;
            timespanstring += $"{timepart:D2}";

            ClockTextValue.Text = timespanstring;
            return (int)timespan.TotalSeconds;
        }
    }
}
