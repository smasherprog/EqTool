using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly SettingsWindowViewModel SettingsWindowData;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly DPSLogParse dPSLogParse;
        private readonly IAppDispatcher appDispatcher;
        private readonly LogParser logParser;

        public Settings(
            LogParser logParser,
            IAppDispatcher appDispatcher,
            DPSLogParse dPSLogParse,
            EQSpells spells,
            EQToolSettings settings,
            EQToolSettingsLoad toolSettingsLoad,
            SettingsWindowViewModel settingsWindowData,
            SpellWindowViewModel spellWindowViewModel)
        {
            this.logParser = logParser;
            this.appDispatcher = appDispatcher;
            this.dPSLogParse = dPSLogParse;
            this.spells = spells;
            this.settings = settings;
            this.spellWindowViewModel = spellWindowViewModel;
            this.toolSettingsLoad = toolSettingsLoad;
            DataContext = SettingsWindowData = settingsWindowData;
            SettingsWindowData.EqPath = this.settings.DefaultEqDirectory;
            Topmost = true;
            InitializeComponent();
            try
            {
                TryUpdateSettings();
            }
            catch
            {

            }

            TryCheckLoggingEnabled();

            foreach (var item in SettingsWindowData.PlayerClasses)
            {
                var it = item.ToString();
                _ = spellbyclassselection.Items.Add(it);
            }

            themecombobox.ItemsSource = new List<KeyValuePair<string, Themes>>()
            {
                new KeyValuePair<string, Themes>(Themes.Light.ToString(), Themes.Light),
                new KeyValuePair<string, Themes>(Themes.Dark.ToString(), Themes.Dark)
            };
            themecombobox.SelectedValue = settings.Theme;

            var releasemode = false;

#if Release
            releasemode = true;
#endif
            if (releasemode)
            {
                DebuggingStack.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveConfig()
        {
            toolSettingsLoad.Save(settings);
            Properties.Settings.Default.Save();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveConfig();
            base.OnClosing(e);
        }

        private void TryUpdateSettings()
        {
            var logfounddata = FindEq.GetLogFileLocation(new FindEq.FindEQData { EqBaseLocation = settings.DefaultEqDirectory, EQlogLocation = string.Empty });
            if (logfounddata?.Found == true)
            {
                settings.EqLogDirectory = logfounddata.Location;
                SettingsWindowData.EqLogPath = logfounddata.Location;
            }
            SettingsWindowData.Update();
            BestGuessSpells.IsChecked = settings.BestGuessSpells;
            YouSpellsOnly.IsChecked = settings.YouOnlySpells;
            var player = SettingsWindowData.ActivePlayer.Player;
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
            SettingsWindowData.IsLoggingEnabled = FindEq.TryCheckLoggingEnabled(settings.DefaultEqDirectory) ?? false;
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SaveConfig();
        }

        private void EqFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog() { RootFolder = Environment.SpecialFolder.MyComputer, Description = "Select Project 1999 EQ Directory", ShowNewFolderButton = false })
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsProject1999Folder(fbd.SelectedPath))
                    {
                        SettingsWindowData.EqPath = settings.DefaultEqDirectory = fbd.SelectedPath;
                        TryUpdateSettings();
                        TryCheckLoggingEnabled();
                    }
                    else
                    {
                        _ = System.Windows.Forms.MessageBox.Show("eqgame.exe was not found in this folder. Make sure this is your project 1999 Folder!", "Message");
                    }
                }
            }
        }

        private void enableLogging_Click(object sender, RoutedEventArgs e)
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
            TryUpdateSettings();
            TryCheckLoggingEnabled();
        }

        private void SaveAlwaysOntopCheckBoxSettings(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            App.ApplyAlwaysOnTop();
        }

        private void SaveSettings(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SaveConfig();
        }

        private void YouSpells_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as System.Windows.Controls.CheckBox;
            settings.YouOnlySpells = s.IsChecked ?? false;
            SaveConfig();
        }

        private void GuessSpells_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as System.Windows.Controls.CheckBox;
            settings.BestGuessSpells = s.IsChecked ?? false;
            SaveConfig();
        }

        private void testspellsclicked(object sender, RoutedEventArgs e)
        {
            var listofspells = new List<SpellParsingMatch>
            {
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Lesser Shielding"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shadow Compact"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Improved Invis to Undead"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Grim Aura"), TargetName = "Joe", MultipleMatchesFound = false },

                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = EQSpells.SpaceYou, MultipleMatchesFound = false },

                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "bob", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast"), TargetName = "bob", MultipleMatchesFound = true },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shield of Words"), TargetName = "bob", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Boon of the Clear Mind"), TargetName = "bob", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Gift of Brilliance"), TargetName = "bob", MultipleMatchesFound = false },

            };

            foreach (var item in listofspells)
            {
                spellWindowViewModel.TryAdd(item);
            }

            spellWindowViewModel.TryAddCustom(new LogCustomTimer.CustomerTimer { DurationInSeconds = 60 * 27, Name = "King" });
            spellWindowViewModel.TryAddCustom(new LogCustomTimer.CustomerTimer { DurationInSeconds = 60 * 18, Name = "hall Wanderer 1" });
        }

        private void spellbyclassselection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var player = SettingsWindowData.ActivePlayer.Player;
            if (player != null)
            {
                player.ShowSpellsForClasses = new List<PlayerClasses>();
                foreach (var item in spellbyclassselection.SelectedItems)
                {
                    player.ShowSpellsForClasses.Add((PlayerClasses)Enum.Parse(typeof(PlayerClasses), item.ToString()));
                }
                SaveConfig();
            }
        }

        private void themescombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.Theme = (themecombobox.SelectedValue as Themes?) ?? Themes.Light;
            App.Theme = settings.Theme;
            SaveConfig();
        }

        private void zoneselectionchanged(object sender, SelectionChangedEventArgs e)
        {
            var player = SettingsWindowData.ActivePlayer.Player;
            if (player != null)
            {
                var t = DateTime.Now;
                var format = "ddd MMM dd HH:mm:ss yyyy";
                var msg = "[" + t.ToString(format) + "] You have entered " + player.Zone;
                logParser.Push(msg);
                SaveConfig();
            }
        }

        private void testDPS(object sender, RoutedEventArgs e)
        {
            var testdpsbutton = sender as System.Windows.Controls.Button;
            if (!testdpsbutton.IsEnabled)
            {
                return;
            }
            testdpsbutton.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                try
                {

                    var fightlines = Properties.Resources.TestFight2.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var fightlist = new List<KeyValuePair<string, DPSParseMatch>>();
                    foreach (var item in fightlines)
                    {
                        if (item == null || item.Length < 27)
                        {
                            continue;
                        }

                        var date = item.Substring(1, 24);
                        var message = item.Substring(27).Trim();
                        var format = "ddd MMM dd HH:mm:ss yyyy";
                        var timestamp = DateTime.Now;
                        try
                        {
                            timestamp = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                        }
                        var match = dPSLogParse.Match(message, timestamp);
                        if (match != null)
                        {
                            fightlist.Add(new KeyValuePair<string, DPSParseMatch>(item, match));
                        }
                    }

                    var endtime = fightlist.LastOrDefault().Value.TimeStamp;
                    var starttime = fightlist.FirstOrDefault().Value.TimeStamp;
                    var starttimediff = DateTime.Now - starttime;
                    var index = 0;
                    do
                    {
                        for (; index < fightlist.Count; index++)
                        {
                            var t = fightlist[index].Value.TimeStamp + starttimediff;
                            if (t > DateTime.Now)
                            {
                                break;
                            }
                            else
                            {
                                var line = fightlist[index].Key;
                                var indexline = line.IndexOf("]");
                                var msgwithout = line.Substring(indexline);
                                var format = "ddd MMM dd HH:mm:ss yyyy";
                                msgwithout = "[" + t.ToString(format) + msgwithout;
                                logParser.Push(msgwithout);
                            }
                        }
                        Thread.Sleep(100);
                    } while (index < fightlist.Count);
                    appDispatcher.DispatchUI(() => { testdpsbutton.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { testdpsbutton.IsEnabled = true; });
                }
            });
        }

        private void mobconclicked(object sender, RoutedEventArgs e)
        {
            var format = "ddd MMM dd HH:mm:ss yyyy";
            var d = DateTime.Now;
            var line = "[" + d.ToString(format) + "] Phinigel Autropos regards you indifferently -- You could probably win this fight.";
            logParser.Push(line);
        }

        private void textmapclicked(object sender, RoutedEventArgs e)
        {
            var testmap = sender as System.Windows.Controls.Button;
            if (!testmap.IsEnabled)
            {
                return;
            }
            testmap.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    NewMethod(" You have entered Cabilis East.", 2000);
                    NewMethod(" Your Location is 1159.11, -595.94, 3.75", 2000);
                    NewMethod(" Your Location is 1170.11, -595.94, 3.75", 2000);
                    NewMethod(" Your Location is 1190.11, -595.94, 3.75", 2000);
                    NewMethod(" Your Location is 1210.11, -595.94, 3.75", 2000);
                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
            });

            void NewMethod(string msg, int sleeptime)
            {
                var format = "ddd MMM dd HH:mm:ss yyyy";
                var d = DateTime.Now;
                var line = "[" + d.ToString(format) + "]" + msg;
                logParser.Push(line);
                Thread.Sleep(sleeptime);
            }
        }

    }
}
