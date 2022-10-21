using EQTool.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public class EQNameValue
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }

        public ObservableCollection<EQNameValue> FontSizes = new ObservableCollection<EQNameValue>();
        public ObservableCollection<EQNameValue> Levels = new ObservableCollection<EQNameValue>();
        public string EqPath = EqToolSettings.BestGuessRootEqPath;
        public string CharName { get; set; }
        public int CharLevel { get; set; }

        public Settings()
        {
            var directory = new DirectoryInfo(Models.EqToolSettings.BestGuessRootEqPath + "/Logs/");
            var loggedincharlogfile = directory.GetFiles()
                .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                .OrderByDescending(a => a.LastWriteTime)
                .FirstOrDefault();
            if (loggedincharlogfile != null)
            {
                var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                var indexpart = charname.IndexOf("_");
                CharName = charname.Substring(0, indexpart);
            }

            InitializeComponent();

            for (var i = 12; i < 72; i++)
            {
                FontSizes.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            for (var i = 1; i < 61; i++)
            {
                Levels.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            levelscombobox.ItemsSource = Levels;
            var players = Properties.Settings.Default.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var level = players.FirstOrDefault(a => a.Name == CharName)?.Level;
            if (!level.HasValue || level <= 0 || level > 60)
            {
                level = 1;
            }

            levelscombobox.SelectedValue = level.ToString();

            fontsizescombobox.ItemsSource = FontSizes;
            fontsizescombobox.SelectedValue = App.GlobalFontSize.ToString();
            DataContext = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var players = Properties.Settings.Default.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var player = players.FirstOrDefault(a => a.Name == CharName);
            if (player != null)
            {
                player.Level = CharLevel;
            }
            else
            {
                players.Add(new PlayerInfo
                {
                    Level = CharLevel,
                    Name = CharName
                });
            }

            Properties.Settings.Default.Players = players;
            Properties.Settings.Default.FontSize = EqToolSettings.FontSize;
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Debug.WriteLine(fontsizescombobox.SelectedValue);
            App.GlobalFontSize = double.Parse(fontsizescombobox.SelectedValue as string);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void levelscombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CharLevel = int.Parse(levelscombobox.SelectedValue as string);
        }
    }
}
