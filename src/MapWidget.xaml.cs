using System.Windows.Controls;
using System.Windows.Media;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWidget.xaml
    /// </summary>
    public partial class MapWidget : UserControl
    {
        public MapWidget(int timer)
        {
            InitializeComponent();
            ClockTextValue.Text = timer.ToString();
        }

        public void SetTheme(Brush labelb, Brush backgroundb)
        {
            ClockText.Foreground = labelb;
            ClockTextValue.Foreground = labelb;
            Background = backgroundb;
        }
    }
}
