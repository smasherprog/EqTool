using EQTool.Models;
using EQTool.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public SettingsWindowData SettingsWindowData = new SettingsWindowData();

        public Settings()
        {
            SettingsWindowData.EqPath = Properties.Settings.Default.DefaultEqDirectory;
            DataContext = SettingsWindowData;
            InitializeComponent();
            TryUpdateCharName();
            TryCheckLoggingEnabled();
            fileopenbuttonimage.Source = Convert(Properties.Resources.open_folder);
            for (var i = 12; i < 72; i++)
            {
                SettingsWindowData.FontSizes.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            for (var i = 1; i < 61; i++)
            {
                SettingsWindowData.Levels.Add(new EQNameValue
                {
                    Name = i.ToString(),
                    Value = i
                });
            }
            levelscombobox.ItemsSource = SettingsWindowData.Levels;
            var players = Properties.Settings.Default.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var level = players.FirstOrDefault(a => a.Name == SettingsWindowData.CharName)?.Level;
            if (!level.HasValue || level <= 0 || level > 60)
            {
                level = 1;
            }

            levelscombobox.SelectedValue = level.ToString();
            fontsizescombobox.ItemsSource = SettingsWindowData.FontSizes;
            fontsizescombobox.SelectedValue = App.GlobalFontSize.ToString();
            BestGuessSpells.IsChecked = Properties.Settings.Default.BestGuessSpells;
        }

        private BitmapImage Convert(Bitmap src)
        {
            using (var memory = new MemoryStream())
            {
                src.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var players = Properties.Settings.Default.Players ?? new List<PlayerInfo>();
            if (!string.IsNullOrWhiteSpace(SettingsWindowData.CharName))
            {
                var player = players.FirstOrDefault(a => a.Name == SettingsWindowData.CharName);
                if (player != null)
                {
                    player.Level = SettingsWindowData.CharLevel;
                }
                else
                {
                    players.Add(new PlayerInfo
                    {
                        Level = SettingsWindowData.CharLevel,
                        Name = SettingsWindowData.CharName
                    });
                }

                Properties.Settings.Default.Players = players;
            }

            Properties.Settings.Default.FontSize = Properties.Settings.Default.FontSize;
            Properties.Settings.Default.GlobalTriggerWindowOpacity = Properties.Settings.Default.GlobalTriggerWindowOpacity;
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        private void TryUpdateCharName()
        {
            try
            {
                var directory = new DirectoryInfo(Properties.Settings.Default.DefaultEqDirectory + "/Logs/");
                var loggedincharlogfile = directory.GetFiles()
                    .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                    .OrderByDescending(a => a.LastWriteTime)
                    .FirstOrDefault();
                if (loggedincharlogfile != null)
                {
                    var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                    var indexpart = charname.IndexOf("_");
                    SettingsWindowData.CharName = charname.Substring(0, indexpart);
                }
            }
            catch { }
        }

        private bool IsEqRunning()
        {
            return Process.GetProcessesByName("eqgame").Length > 0;
        }

        private void TryCheckLoggingEnabled()
        {
            try
            {
                SettingsWindowData.IsLogginEnabled = false;
                var data = File.ReadAllLines(Properties.Settings.Default.DefaultEqDirectory + "/eqclient.ini");
                foreach (var item in data)
                {
                    var line = item.ToLower().Trim().Replace(" ", string.Empty);
                    if (line.StartsWith("log="))
                    {
                        SettingsWindowData.IsLogginEnabled = line.Contains("true");
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Debug.WriteLine(fontsizescombobox.SelectedValue);
            App.GlobalFontSize = double.Parse(fontsizescombobox.SelectedValue as string);
        }

        private void DoneButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void levelscombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SettingsWindowData.CharLevel = int.Parse(levelscombobox.SelectedValue as string);
        }

        private void EqFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsValid(fbd.SelectedPath))
                    {
                        SettingsWindowData.EqPath = Properties.Settings.Default.DefaultEqDirectory = fbd.SelectedPath;
                        TryUpdateCharName();
                        TryCheckLoggingEnabled();
                    }
                    else
                    {
                        _ = System.Windows.Forms.MessageBox.Show("eqgame.exe was not found in this folder!", "Message");
                    }
                }
            }
        }

        private void enablelogging_Click(object sender, RoutedEventArgs e)
        {
            if (IsEqRunning())
            {
                _ = System.Windows.MessageBox.Show("You must exit EQ before you can enable Logging!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var data = File.ReadAllLines(Properties.Settings.Default.DefaultEqDirectory + "/eqclient.ini");
                var newlist = new List<string>();
                foreach (var item in data)
                {

                    var line = item.ToLower().Trim().Replace(" ", string.Empty);
                    if (line.StartsWith("log="))
                    {
                        newlist.Add("Log=TRUE");
                    }
                    else
                    {
                        newlist.Add(item);
                    }
                }
                File.WriteAllLines(Properties.Settings.Default.DefaultEqDirectory + "/eqclient.ini", newlist);
            }
            catch { }
        }

        private void GlobalTriggerWindowOpacityValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.GlobalTriggerWindowOpacity = App.GlobalTriggerWindowOpacity = (sender as Slider).Value;
        }

        private void GuessSpells_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BestGuessSpells = true;
        }

        private void GuessSpells_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BestGuessSpells = false;
        }
    }
}
