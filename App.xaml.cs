using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static double GlobalFontSize
        {
            get => (double)Current.Resources["GlobalFontSize"];
            set => Current.Resources["GlobalFontSize"] = value;
        }
    }
}
