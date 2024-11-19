﻿using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQTool.Services.P99LoginMiddlemand;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EQTool.UI.SettingsComponents
{
    public partial class SettingsGeneral : UserControl
    {
        private readonly SettingsWindowViewModel SettingsWindowData;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly EQSpells spells;
        private readonly DamageParser damageLogParser;
        private readonly CommsParser playerCommsParser;
        private readonly IAppDispatcher appDispatcher;
        private readonly ISignalrPlayerHub signalrPlayerHub;
        private readonly LogParser logParser;
        private readonly MapLoad mapLoad;
        private readonly LoginMiddlemand loginMiddlemand;
        private readonly SettingsTestRunOverlay settingsTestRunOverlay;
        private readonly WindowFactory windowFactory;
        private readonly LogEvents logEvents;

        public SettingsGeneral(
            LogEvents logEvents,
            WindowFactory windowFactory,
            LogParser logParser,
            MapLoad mapLoad,
            IAppDispatcher appDispatcher,
            ISignalrPlayerHub signalrPlayerHub,
            DamageParser damageLogParse,
            CommsParser playerCommsParser,
            EQSpells spells,
            LoginMiddlemand loginMiddlemand,
            EQToolSettings settings,
            EQToolSettingsLoad toolSettingsLoad,
            SettingsWindowViewModel settingsWindowData,
            SettingsTestRunOverlay settingsTestRunOverlay)
        {
            this.loginMiddlemand = loginMiddlemand;
            this.signalrPlayerHub = signalrPlayerHub;
            this.logParser = logParser;
            this.logEvents = logEvents;
            this.mapLoad = mapLoad;
            this.windowFactory = windowFactory;
            this.appDispatcher = appDispatcher;
            damageLogParser = damageLogParse;
            this.playerCommsParser = playerCommsParser;
            this.spells = spells;
            this.settings = settings;
            this.toolSettingsLoad = toolSettingsLoad;
            DataContext = SettingsWindowData = settingsWindowData;
            SettingsWindowData.EqPath = this.settings.DefaultEqDirectory;
            InitializeComponent();
            TryCheckLoggingEnabled();
            try
            {
                TryUpdateSettings();
            }
            catch
            {

            }
            DebugTab.Visibility = Visibility.Collapsed;
            this.settingsTestRunOverlay = settingsTestRunOverlay;
#if DEBUG
            DebugTab.Visibility = Visibility.Visible;
#endif

        }

        private void SaveConfig()
        {
            toolSettingsLoad.Save(settings);
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
            if (loginMiddlemand.Running)
            {
                LoginMiddleMandCheckBox.IsChecked = true;
            }
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
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
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog() { Description = descriptiontext, ShowNewFolderButton = false })
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsValidEqFolder(fbd.SelectedPath))
                    {
                        SettingsWindowData.EqPath = settings.DefaultEqDirectory = fbd.SelectedPath;
                        appDispatcher.DispatchUI(() =>
                        {
                            TryUpdateSettings();
                            TryCheckLoggingEnabled();
                        });
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

        private void Savesettings(object sender, RoutedEventArgs e)
        {
            SaveConfig();
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
            var listofspells = new List<SpellCastEvent>
            {
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Lesser Shielding"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shadow Compact"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Improved Invis vs Undead"), TargetName = "Joe", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Grim Aura"), TargetName = "Joe", MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = EQSpells.SpaceYou, MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Heroic Bond"), TargetName = "Aasgard", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast"), TargetName = "Aasgard", MultipleMatchesFound = true },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Shield of Words"), TargetName = "Aasgard", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Boon of the Clear Mind"), TargetName = "Aasgard", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Gift of Brilliance"), TargetName = "Aasgard", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Mana Sieve"), TargetName = "a bad guy", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Mana Sieve"), TargetName = "a bad guy", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Harvest"), TargetName = EQSpells.SpaceYou, MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "LowerElement"), TargetName = "Tunare", MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Concussion"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Concussion"), TargetName = "Tunare", MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Flame Lick"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Flame Lick"), TargetName = "Tunare", MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Jolt"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Jolt"), TargetName = "Tunare", MultipleMatchesFound = false },

                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Cinder Jolt"), TargetName = "Tunare", MultipleMatchesFound = false },
                new SpellCastEvent { Spell = spells.AllSpells.FirstOrDefault(a => a.name == "Cinder Jolt"), TargetName = "Tunare", MultipleMatchesFound = false }
            };

            foreach (var item in listofspells)
            {
                logEvents.Handle(item);
            }
            PushLog("Fright says, 'Luetin'");
            PushLog("You say, 'PigTimer-6:40-Guard_George'");
            PushLog("You say, 'PigTimer-1:06:40-King_Wanderer'");
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
                    _ = player.ShowSpellsForClasses.Remove(item);
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

        private void testRandomRolls(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.RandomRollEvent);
        }

        private void testDPS(object sender, RoutedEventArgs e)
        {
            var testdpsbutton = sender as System.Windows.Controls.Button;
            if (!testdpsbutton.IsEnabled)
            {
                return;
            }
            //  testdpsbutton.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var fightlines = Properties.Resources.TestFight2.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var fightlist = new List<KeyValuePair<string, DamageEvent>>();
                    var linecounter = 0;
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
                        var match = damageLogParser.Match(message, timestamp, linecounter++);
                        if (match != null)
                        {
                            fightlist.Add(new KeyValuePair<string, DamageEvent>(item, match));
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
            PushLog(logtext);
        }

        private void PushLog(string message)
        {
            var logtext = message?.Trim();
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
            logParser.Push(logtext);
        }

        private void selectLogFile(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (!button.IsEnabled)
            {
                return;
            }
            var dialog = new Microsoft.Win32.OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result == true)
            {

                var filename = dialog.FileName;
                button.IsEnabled = false;
                _ = Task.Factory.StartNew(() =>
                {
                    var fightlines = File.ReadAllLines(filename);
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

                        appDispatcher.DispatchUI(() => { button.IsEnabled = true; });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        appDispatcher.DispatchUI(() => { button.IsEnabled = true; });
                    }
                });
            }
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

            var player = SettingsWindowData.ActivePlayer?.Player;
            if (player == null)
            {
                return;
            }

            var map = mapLoad.Load(player.Zone);
            if (map == null)
            {
                return;
            }

            testmap.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                var names = new List<string>() { "faiil", "irishfaf", "chuunt", "jakab", "nima", "healmin" };
                var pnames = new List<EQToolShared.Map.SignalrPlayer>();

                var movementoffset = (int)(map.AABB.MaxWidth / 10);
                var r = new Random();
                var offset = r.Next(-movementoffset, movementoffset);
                foreach (var item in names)
                {
                    offset = r.Next(-movementoffset, movementoffset);
                    var p = new EQToolShared.Map.SignalrPlayer
                    {
                        GuildName = "The Drift",
                        MapLocationSharing = EQToolShared.Map.MapLocationSharing.Everyone,
                        Name = item,
                        PlayerClass = PlayerClasses.Necromancer,
                        Server = Servers.Green,
                        Zone = player.Zone,
                        X = map.AABB.Center.X + map.Offset.X + r.Next(-movementoffset, movementoffset),
                        Y = map.AABB.Center.Y + map.Offset.Y + r.Next(-movementoffset, movementoffset),
                        Z = map.AABB.Center.Z + map.Offset.Z + r.Next(-movementoffset, movementoffset)
                    };

                    pnames.Add(p);
                    signalrPlayerHub.PushPlayerLocationEvent(p);
                }
                movementoffset = (int)(map.AABB.MaxWidth / 50);
                try
                {
                    var starttime = DateTime.UtcNow;
                    while ((starttime - DateTime.UtcNow).TotalMinutes < 3)
                    {
                        foreach (var item in pnames)
                        {
                            var p = new EQToolShared.Map.SignalrPlayer
                            {
                                GuildName = "The Drift",
                                MapLocationSharing = EQToolShared.Map.MapLocationSharing.Everyone,
                                Name = item.Name,
                                PlayerClass = PlayerClasses.Necromancer,
                                Server = Servers.Green,
                                Zone = player.Zone,
                                X = item.X + r.Next(-movementoffset, movementoffset),
                                Y = item.Y + r.Next(-movementoffset, movementoffset),
                                Z = item.Z + r.Next(-movementoffset, movementoffset)
                            };

                            signalrPlayerHub.PushPlayerLocationEvent(p);
                        }
                        Thread.Sleep(1000);
                    }

                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { testmap.IsEnabled = true; });
                }
            });
        }

        private void CHTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveConfig();
        }
        private void SaveAlwaysOntopCheckBoxSettings(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            ((App)System.Windows.Application.Current).ApplyAlwaysOnTop();
        }

        private void testenrage(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.EnrageEvent);
        }

        private void testlevfading(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.LevitateEvent);
        }
        private void testinvisfading(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.InvisEvent);
        }

        private void testCharmBreak(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.CharmBreakEvent);
        }

        private void testFailedFeign(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.FailedFeignEvent);
        }

        private void testFTE(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.FTEEvent);
        }

        private void testGroupInvite(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.GroupInviteEvent);
        }

        private void testDragonRoar(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.DragonFearEvent);
        }
        private void textvoice(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SettingsWindowData.SelectedVoice))
            {
                return;
            }
#if !LINUX
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                synth.SelectVoice(SettingsWindowData.SelectedVoice);
                synth.Speak($"You resist the Dragon Roar spell!");
            });
#endif
        }

        private void testChChain(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (!button.IsEnabled)
            {
                return;
            }
            if (SettingsWindowData.ActivePlayer?.Player == null)
            {
                return;
            }
            SettingsWindowData.ActivePlayer.Player.ChChainOverlay = true;
            SettingsWindowData.ActivePlayer.Player.ChChainWarningOverlay = true;
            SettingsWindowData.ActivePlayer.Player.ChChainWarningAudio = true;
            ((App)System.Windows.Application.Current).OpenOverLayWindow();
            button.IsEnabled = false;
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var tag = SettingsWindowData.ActivePlayer.Player.ChChainTagOverlay;
                    var msg = $"You shout, '{tag} 001 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(2000);

                    msg = $"Mycro shouts, '{tag}  002 CH -- Huntor'";
                    PushLog(msg);
                    Thread.Sleep(1000);

                    msg = $"Sleeper shouts, '{tag}  003 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(2000);

                    msg = $"Sleeper shouts, '{tag}  004 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(1800);

                    msg = $"You shout, '{tag}  001 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(2000);

                    msg = $"Mycro shouts, '{tag}  002 CH -- Huntor'";
                    PushLog(msg);
                    Thread.Sleep(1500);

                    msg = $"Sleeper shouts, '{tag}  003 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(2000);

                    msg = $"Sleeper shouts, '{tag}  004 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(1700);

                    msg = $"Hanbox shouts, '{tag}  002 CH -- Huntor'";
                    PushLog(msg);
                    Thread.Sleep(1500);

                    msg = $"Hanbox shouts, '{tag}  001 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(1700);

                    msg = $"Sleeper shouts, '{tag}  003 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(2000);

                    msg = $"Sleeper shouts, '{tag}  004 CH -- Beefwich'";
                    PushLog(msg);
                    Thread.Sleep(1800);

                    msg = $"Mycro shouts, '{tag}  002 CH -- Huntor'";
                    PushLog(msg);
                    Thread.Sleep(1500);
                    appDispatcher.DispatchUI(() => { button.IsEnabled = true; });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    appDispatcher.DispatchUI(() => { button.IsEnabled = true; });
                }
            });
        }

        private void testRootBreak(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.RootBreakEvent);
        }

        private void testResists(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.ResistSpellEvent);
        }

        private void LoginMiddleMandToggle(object sender, RoutedEventArgs e)
        {
            var s = sender as System.Windows.Controls.CheckBox;
            if (s.IsChecked == true)
            {
                if (loginMiddlemand.IsConfiguredCorrectly())
                {
                    loginMiddlemand.StartListening();
                    settings.LoginMiddleMand = true;
                    SaveConfig();
                }
                else
                {
                    var result = System.Windows.MessageBox.Show("Your eqhost.txt file is not setup correctly to use LoginMiddlemand. Would you like to use default settings?", "LoginMiddlemand Configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        loginMiddlemand.ApplyDefaultConfiguration();
                        settings.LoginMiddleMand = true;
                        SaveConfig();

                        if (IsEqRunning())
                        {
                            _ = System.Windows.MessageBox.Show("You must exit EQ before you can start using LoginMiddlemand! Please close Everquest and restart it!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        loginMiddlemand.StartListening();
                    }
                    else
                    {
                        s.IsChecked = false;
                    }
                }
            }
            else
            {
                settings.LoginMiddleMand = false;
                SaveConfig();
                var result = System.Windows.MessageBox.Show("Would you like your eqhosts.txt file to be reverted to use eqemulator default login server?", "Revert LoginMiddlemand Configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    loginMiddlemand.RevertDefaultConfiguration();
                    if (IsEqRunning())
                    {
                        _ = System.Windows.MessageBox.Show("You must exit EQ before Everquest will use these settings! Please close Everquest and restart it!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                loginMiddlemand.StopListening();
            }
        }

        private void testDeathLoop(object sender, RoutedEventArgs e)
        {
            settingsTestRunOverlay.RunTest(OverlayTypes.DeathLoopEvent);
        }

        private void openSpawnTimerDialog(object sender, RoutedEventArgs e)
        {
            _ = windowFactory.CreateWindow<SpawnTimerDialog>().ShowDialog();
        }

        private void selectallVisual(object sender, RoutedEventArgs e)
        {
            SettingsWindowData.ActivePlayer.Player.EnrageOverlay = true;
            SettingsWindowData.ActivePlayer.Player.LevFadingOverlay = true;
            SettingsWindowData.ActivePlayer.Player.InvisFadingOverlay = true;
            SettingsWindowData.ActivePlayer.Player.FTEOverlay = true;
            SettingsWindowData.ActivePlayer.Player.CharmBreakOverlay = true;
            SettingsWindowData.ActivePlayer.Player.FailedFeignOverlay = true;
            SettingsWindowData.ActivePlayer.Player.GroupInviteOverlay = true;
            SettingsWindowData.ActivePlayer.Player.DragonRoarOverlay = true;
            SettingsWindowData.ActivePlayer.Player.RootWarningOverlay = true;
            SettingsWindowData.ActivePlayer.Player.ResistWarningOverlay = true;
            SettingsWindowData.ActivePlayer.Player.DeathLoopOverlay = true;
            SettingsWindowData.ActivePlayer.Player.ChChainOverlay = true;
            SettingsWindowData.ActivePlayer.Player.ChChainWarningOverlay = true;
            SaveConfig();
        }
        private void selectallAudio(object sender, RoutedEventArgs e)
        {
            SettingsWindowData.ActivePlayer.Player.EnrageAudio = true;
            SettingsWindowData.ActivePlayer.Player.LevFadingAudio = true;
            SettingsWindowData.ActivePlayer.Player.InvisFadingAudio = true;
            SettingsWindowData.ActivePlayer.Player.FTEAudio = true;
            SettingsWindowData.ActivePlayer.Player.CharmBreakAudio = true;
            SettingsWindowData.ActivePlayer.Player.FailedFeignAudio = true;
            SettingsWindowData.ActivePlayer.Player.GroupInviteAudio = true;
            SettingsWindowData.ActivePlayer.Player.DragonRoarAudio = true;
            SettingsWindowData.ActivePlayer.Player.RootWarningAudio = true;
            SettingsWindowData.ActivePlayer.Player.ResistWarningAudio = true;
            SettingsWindowData.ActivePlayer.Player.DeathLoopAudio = true;
            SettingsWindowData.ActivePlayer.Player.ChChainWarningAudio = true;
            SaveConfig();
        }

        private void OpenManagementWindow(object sender, RoutedEventArgs e)
        {
            windowFactory.CreateWindow<SettingManagement>().Show();
        }
    }
}