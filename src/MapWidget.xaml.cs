using EQTool.Services;
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
        public TimerInfo TimerInfo { get; private set; }    
        public MapWidget(TimerInfo timerInfo)
        {
            InitializeComponent();
            TimerInfo = timerInfo;
            ClockTextValue.FontSize = TimerInfo.Fontsize;  
            _ = Update();
            Label.Text = TimerInfo.Name; 
        }

        public void SetTheme(Brush labelb, Brush backgroundb)
        { 
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
            var timespan = TimerInfo.EndTime - DateTime.Now;
            var timespanstring = string.Empty;
            if (timespan.TotalSeconds <= 0)
            {
                ClockTextValue.Text = "00:00:00";
                Background = new SolidColorBrush(GetColorFromRedYellowGreenGradient(0));
                return (int)timespan.TotalSeconds;
            }
            var percentleft = timespan.TotalSeconds / TimerInfo.Duration.TotalSeconds * 100;
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
