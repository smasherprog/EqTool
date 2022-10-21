using EQTool.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public class EqFontSize
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }

        public ObservableCollection<EqFontSize> FontSizes = new ObservableCollection<EqFontSize>();
        public string EqPath = EqToolSettings.BestGuessRootEqPath;
        public Settings()
        {
            InitializeComponent();
            for (var i = 12; i < 72; i++)
            {
                FontSizes.Add(new EqFontSize
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            fontsizescombobox.ItemsSource = FontSizes;
            fontsizescombobox.SelectedValue = App.GlobalFontSize.ToString();
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Debug.WriteLine(fontsizescombobox.SelectedValue);
            App.GlobalFontSize = double.Parse(fontsizescombobox.SelectedValue as string);
        }
    }
}
