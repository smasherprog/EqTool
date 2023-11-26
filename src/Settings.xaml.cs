using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace EQTool
{
    public class BoolStringClass : INotifyPropertyChanged
    {
        public string TheText { get; set; }

        public PlayerClasses TheValue { get; set; }

        private bool _IsChecked { get; set; }

        public bool IsChecked
        {
            get => _IsChecked; set
            {
                _IsChecked = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class Settings : Window
    {
        private readonly SettingsWindowViewModel SettingsWindowData;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly DPSLogParse dPSLogParse;
        private readonly IAppDispatcher appDispatcher;
        private readonly ISignalrPlayerHub signalrPlayerHub;
        private readonly LogParser logParser;

        public Settings(
            LogParser logParser,
            IAppDispatcher appDispatcher,
            ISignalrPlayerHub signalrPlayerHub,
            DPSLogParse dPSLogParse,
            EQSpells spells,
            EQToolSettings settings,
            EQToolSettingsLoad toolSettingsLoad,
            SettingsWindowViewModel settingsWindowData,
            SpellWindowViewModel spellWindowViewModel)
        {
            this.signalrPlayerHub = signalrPlayerHub;
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
            TryCheckLoggingEnabled();
            var releasemode = false;

#if RELEASE
            releasemode = true;
#endif
            if (releasemode)
            {
                DebuggingStack.Visibility = Visibility.Collapsed;
            }

            try
            {
                TryUpdateSettings();
            }
            catch
            {

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

            if (player?.ShowSpellsForClasses != null)
            {
                foreach (var item in SettingsWindowData.SelectedPlayerClasses)
                {
                    item.IsChecked = player.ShowSpellsForClasses.Contains(item.TheValue);
                }
            }
            else
            {
                foreach (var item in SettingsWindowData.SelectedPlayerClasses)
                {
                    item.IsChecked = false;
                }
            }
            var hasvalideqdir = FindEq.IsValidEqFolder(settings.DefaultEqDirectory);
            if (hasvalideqdir && FindEq.TryCheckLoggingEnabled(settings.DefaultEqDirectory) == true)
            {
                ((App)System.Windows.Application.Current).ToggleMenuButtons(true);
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
            var descriptiontext = "Select Project 1999 EQ Directory";
#if QUARM
            descriptiontext = "Select Quarm EQ Directory";
#endif

            using (var fbd = new FolderBrowserDialog() { Description = descriptiontext, ShowNewFolderButton = false })
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsValidEqFolder(fbd.SelectedPath))
                    {
                        SettingsWindowData.EqPath = settings.DefaultEqDirectory = fbd.SelectedPath;
                        TryUpdateSettings();
                        TryCheckLoggingEnabled();
                    }
                    else
                    {
                        _ = System.Windows.Forms.MessageBox.Show("eqgame.exe was not found in this folder. Make sure this is a valid Folder!", "Message");
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
                if (settings.DefaultEqDirectory.ToLower().Contains("program files"))
                {
                    _ = System.Windows.MessageBox.Show("Everquest is installed in program files. YOU MUST ADD LOG=TRUE to the eqclient.ini yourself.", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
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
            }
            catch { }
            TryUpdateSettings();
            TryCheckLoggingEnabled();
        }

        private void SaveAlwaysOntopCheckBoxSettings(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            ((App)System.Windows.Application.Current).ApplyAlwaysOnTop();
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
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Mana Sieve"), TargetName = "a bad guy", MultipleMatchesFound = false },
                new SpellParsingMatch { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Mana Sieve"), TargetName = "a bad guy", MultipleMatchesFound = false },
            };

            foreach (var item in listofspells)
            {
                spellWindowViewModel.TryAdd(item);
            }

            spellWindowViewModel.TryAddCustom(new CustomTimer { DurationInSeconds = 60 * 27, Name = "King" });
            spellWindowViewModel.TryAddCustom(new CustomTimer { DurationInSeconds = 60 * 18, Name = "hall Wanderer 1" });
        }

        private void CheckBoxZone_Checked(object sender, RoutedEventArgs e)
        {
            var chkZone = (System.Windows.Controls.CheckBox)sender;
            var player = SettingsWindowData.ActivePlayer.Player;
            if (player != null)
            {
                var item = (PlayerClasses)chkZone.Tag;
                if (chkZone.IsChecked == true && !player.ShowSpellsForClasses.Any(a => a == item))
                {
                    player.ShowSpellsForClasses.Add(item);
                    SaveConfig();
                }
                else if (chkZone.IsChecked == false && player.ShowSpellsForClasses.Any(a => a == item))
                {
                    player.ShowSpellsForClasses.Remove(item);
                    SaveConfig();
                }
            }
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

        private void logpush(object sender, RoutedEventArgs e)
        {
            var logtext = LogPushText.Text?.Trim();
            if (string.IsNullOrWhiteSpace(logtext))
            {
                return;
            }
            if (!logtext.StartsWith("["))
            {
                var format = "ddd MMM dd HH:mm:ss yyyy";
                var d = DateTime.Now;
                logtext = "[" + d.ToString(format) + "] " + logtext;
            }
            LogPushText.Text = string.Empty;
            logParser.Push(logtext);
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
                var fightlines = Properties.Resources.testmap.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var lines = new List<KeyValuePair<DateTime, string>>();
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
                        lines.Add(new KeyValuePair<DateTime, string>(timestamp, item));
                    }
                    catch (FormatException)
                    {
                    }
                }
                var starttime = lines.FirstOrDefault().Key;
                try
                {

                    var starttimediff = DateTime.Now - starttime;
                    var index = 0;
                    do
                    {
                        for (; index < lines.Count; index++)
                        {
                            var t = lines[index].Key + starttimediff;
                            if (t > DateTime.Now)
                            {
                                break;
                            }
                            else
                            {
                                var line = lines[index].Value;
                                var indexline = line.IndexOf("]");
                                var msgwithout = line.Substring(indexline);
                                var format = "ddd MMM dd HH:mm:ss yyyy";
                                msgwithout = "[" + t.ToString(format) + msgwithout;
                                logParser.Push(msgwithout);
                            }
                        }
                        Thread.Sleep(100);
                    } while (index < lines.Count);

                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
            });
        }

        private void testsignalrlocations(object sender, RoutedEventArgs e)
        {
            var testmap = sender as System.Windows.Controls.Button;
            if (!testmap.IsEnabled)
            {
                return;
            }
            testmap.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                var fightlines = Properties.Resources.testmap.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var lines = new List<KeyValuePair<DateTime, string>>();
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
                        lines.Add(new KeyValuePair<DateTime, string>(timestamp, item));
                    }
                    catch (FormatException)
                    {
                    }
                }
                var starttime = lines.FirstOrDefault().Key;
                try
                {

                    var starttimediff = DateTime.Now - starttime;
                    var index = 0;
                    do
                    {
                        for (; index < lines.Count; index++)
                        {
                            var t = lines[index].Key + starttimediff;
                            if (t > DateTime.Now)
                            {
                                break;
                            }
                            else
                            {
                                var line = lines[index].Value;
                                var search = "Your Location is ";
                                var indexsearch = line.IndexOf(search);
                                if (indexsearch != -1)
                                {
                                    var loc = line.Substring(indexsearch + search.Length)
                                        .Split(',')
                                        .Select(a => a.Trim())
                                        .Where(a => !string.IsNullOrWhiteSpace(a))
                                        .Select(a => double.Parse(a))
                                        .ToList();
                                    signalrPlayerHub.PushPlayerLocationEvent(new EQToolShared.Map.SignalrPlayer
                                    {
                                        GuildName = "The Drift",
                                        MapLocationSharing = EQToolShared.Map.MapLocationSharing.Everyone,
                                        Name = "Vasanle",
                                        PlayerClass = PlayerClasses.Necromancer,
                                        Server = Servers.Green,
                                        Zone = "mischiefplane",
                                        X = loc[0],
                                        Y = loc[1],
                                        Z = loc[2]
                                    });
                                }
                            }
                        }
                        Thread.Sleep(100);
                    } while (index < lines.Count);

                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
            });
        }
    }
}
