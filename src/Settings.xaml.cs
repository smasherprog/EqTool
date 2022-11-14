using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly SettingsWindowData SettingsWindowData;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public Settings(EQSpells spells, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, SettingsWindowData settingsWindowData, SpellWindowViewModel spellWindowViewModel)
        {
            SettingsWindowData = settingsWindowData;
            this.spells = spells;
            this.settings = settings;
            this.spellWindowViewModel = spellWindowViewModel;
            this.toolSettingsLoad = toolSettingsLoad;
            SettingsWindowData.EqPath = this.settings.DefaultEqDirectory;
            DataContext = settingsWindowData;
            InitializeComponent();
            TryUpdateSettings();
            TryCheckLoggingEnabled();
            fileopenbuttonimage.Source = Properties.Resources.open_folder.ConvertToBitmapImage();

            foreach (var item in SettingsWindowData.PlayerClasses)
            {
                var it = item.ToString();
                _ = spellbyclassselection.Items.Add(it);
            }
            levelscombobox.ItemsSource = SettingsWindowData.Levels;
            fontsizescombobox.ItemsSource = SettingsWindowData.FontSizes;
            fontsizescombobox.SelectedValue = App.GlobalFontSize.ToString();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var players = settings.Players ?? new List<PlayerInfo>();
            if (SettingsWindowData.HasCharName)
            {
                var player = players.FirstOrDefault(a => a.Name == SettingsWindowData.CharName);
                if (player != null)
                {
                    player.Level = SettingsWindowData.CharLevel;
                    player.PlayerClass = SettingsWindowData.PlayerClass;
                }
                else
                {
                    player = new PlayerInfo
                    {
                        Level = SettingsWindowData.CharLevel,
                        Name = SettingsWindowData.CharName,
                        PlayerClass = SettingsWindowData.PlayerClass
                    };
                    players.Add(player);
                }

                player.ShowSpellsForClasses = new List<PlayerClasses>();
                foreach (var item in spellbyclassselection.SelectedItems)
                {
                    player.ShowSpellsForClasses.Add((PlayerClasses)Enum.Parse(typeof(PlayerClasses), item.ToString()));
                }

                settings.Players = players;
            }

            settings.FontSize = settings.FontSize;
            settings.GlobalTriggerWindowOpacity = settings.GlobalTriggerWindowOpacity;
            toolSettingsLoad.Save(settings);
            base.OnClosing(e);
        }

        private void TryUpdateSettings()
        {
            try
            {
                var directory = new DirectoryInfo(settings.DefaultEqDirectory + "/Logs/");
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

            var players = settings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var player = players.FirstOrDefault(a => a.Name == SettingsWindowData.CharName);
            var level = player?.Level;
            if (!level.HasValue || level <= 0 || level > 60)
            {
                level = 1;
            }

            if (player != null)
            {
                SettingsWindowData.PlayerClass = player.PlayerClass;
            }

            levelscombobox.SelectedValue = level.ToString();
            BestGuessSpells.IsChecked = settings.BestGuessSpells;
            YouSpellsOnly.IsChecked = settings.YouOnlySpells;
            var selecteditems = new List<string>();
            foreach (var item in SettingsWindowData.PlayerClasses)
            {
                var it = item.ToString();
                if (player?.ShowSpellsForClasses == null || player.ShowSpellsForClasses.Any(a => a == item))
                {
                    selecteditems.Add(it);
                }
            }

            foreach (var item in selecteditems)
            {
                if (!spellbyclassselection.SelectedItems.Contains(item))
                {
                    _ = spellbyclassselection.SelectedItems.Add(item);
                }
            }
        }

        private bool IsEqRunning()
        {
            return Process.GetProcessesByName("eqgame").Length > 0;
        }

        private void TryCheckLoggingEnabled()
        {
            SettingsWindowData.IsLogginEnabled = FindEq.TryCheckLoggingEnabled(settings.DefaultEqDirectory) ?? false;
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Debug.WriteLine(fontsizescombobox.SelectedValue);
            App.GlobalFontSize = double.Parse(fontsizescombobox.SelectedValue as string);
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
                        SettingsWindowData.EqPath = settings.DefaultEqDirectory = fbd.SelectedPath;
                        TryUpdateSettings();
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
                var data = File.ReadAllLines(settings.DefaultEqDirectory + "/eqclient.ini");
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
                File.WriteAllLines(settings.DefaultEqDirectory + "/eqclient.ini", newlist);
            }
            catch { }
        }

        private void GlobalTriggerWindowOpacityValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            settings.GlobalTriggerWindowOpacity = App.GlobalTriggerWindowOpacity = (sender as Slider).Value;
        }
        private void YouSpells_Checked(object sender, RoutedEventArgs e)
        {
            settings.YouOnlySpells = true;
        }

        private void YouSpells_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.YouOnlySpells = false;
        }

        private void GuessSpells_Checked(object sender, RoutedEventArgs e)
        {
            settings.BestGuessSpells = true;
        }

        private void GuessSpells_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.BestGuessSpells = false;
        }

        private void testspellsclicked(object sender, RoutedEventArgs e)
        {
            var listofspells = new List<SpellParsingMatch>
            {
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud"), TargetName = "Joe", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Lesser Shielding"), TargetName = "Joe", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shadow Compact"), TargetName = "Joe", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "Joe", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Improved Invis to Undead"), TargetName = "Joe", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Grim Aura"), TargetName = "Joe", MutipleMatchesFound = false },

                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "bob", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast"), TargetName = "bob", MutipleMatchesFound = true },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shield of Words"), TargetName = "bob", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Boon of the Clear Mind"), TargetName = "bob", MutipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Gift of Brilliance"), TargetName = "bob", MutipleMatchesFound = false },

                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = " You", MutipleMatchesFound = false },
            };

            foreach (var item in listofspells)
            {
                spellWindowViewModel.TryAdd(item);
            }
        }

        private void spellbyclassselection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var players = settings.Players ?? new List<PlayerInfo>();
            if (SettingsWindowData.HasCharName)
            {
                var player = players.FirstOrDefault(a => a.Name == SettingsWindowData.CharName);
                if (player != null)
                {
                    player.ShowSpellsForClasses = new List<PlayerClasses>();
                    foreach (var item in spellbyclassselection.SelectedItems)
                    {
                        player.ShowSpellsForClasses.Add((PlayerClasses)Enum.Parse(typeof(PlayerClasses), item.ToString()));
                    }
                    toolSettingsLoad.Save(settings);
                }
            }
        }
    }
}
