using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class DPSMeter : Window
    {
        public ObservableCollection<EntittyDPS> CurrentEntityList = new ObservableCollection<EntittyDPS>();
        public ObservableCollection<EntittyDPS> LastEntityList = new ObservableCollection<EntittyDPS>();
        private readonly Timer ParseTimer;
        private readonly Timer UITimer;
        private long? LastReadOffset;
        private readonly string HitMessage = " points of damage.";
        private DateTime? LastTimeFighting;
        private readonly EQToolSettings settings;
        private readonly ParseSpells parseSpells;
        private readonly SpellIcons spellIcons;

        public DPSMeter(EQToolSettings settings, ParseSpells parseSpells, SpellIcons spellIcons)
        {
            this.settings = settings;
            this.parseSpells = parseSpells;
            this.spellIcons = spellIcons;
            InitializeComponent();
            ParseTimer = new System.Timers.Timer(500);
            ParseTimer.Elapsed += PollUpdates;
            ParseTimer.Enabled = true;

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;

            currentfight.ItemsSource = CurrentEntityList;
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private FileInfo TryUpdatePlayerLevel()
        {
            var players = settings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var directory = new DirectoryInfo(settings.DefaultEqDirectory + "/Logs/");
            var loggedincharlogfile = directory.GetFiles()
                .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                .OrderByDescending(a => a.LastWriteTime)
                .FirstOrDefault();
            return loggedincharlogfile;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            ParseTimer.Stop();
            UITimer.Dispose();
            ParseTimer.Dispose();
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                var itemstoremove = new List<UISpell>();
                foreach (var item in CurrentEntityList)
                {
                    item.TotalSeconds += 1;
                }
                var now = DateTime.Now;
                if (LastTimeFighting.HasValue && (now - LastTimeFighting.Value).TotalSeconds > 20)
                {
                    CurrentEntityList.Clear();
                }
            });
        }

        private void PollUpdates(object sender, EventArgs e)
        {
            var loggedincharlogfile = TryUpdatePlayerLevel();
            if (string.IsNullOrWhiteSpace(loggedincharlogfile?.FullName))
            {
                return;
            }

            if (!LastReadOffset.HasValue || LastReadOffset >= loggedincharlogfile.Length)
            {
                LastReadOffset = loggedincharlogfile.Length;
            }
            try
            {
                using (var stream = new FileStream(loggedincharlogfile?.FullName, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    LastReadOffset = stream.Seek(LastReadOffset.Value, SeekOrigin.Begin);
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        LastReadOffset = stream.Position;
                        if (line.Length > 27)
                        {
                            ParseLine(line);
                        }
                    }
                }
            }
            catch { }
        }

        private void ParseLine(string line)
        {
            var date = line.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var now = DateTime.Now;
            var message = line.Substring(27);
            Debug.WriteLine(message);
            App.Current.Dispatcher.Invoke(delegate
            {
                if (message.EndsWith(HitMessage))
                {
                    LastTimeFighting = DateTime.Now;
                    message = message.Replace(HitMessage, string.Empty);
                    var splits = message.Split(' ');
                    var damagedone = splits[splits.Length - 1];
                    var hittype = splits[1];
                    var nameofchar = splits[0];
                    var afterhit = message.IndexOf(hittype);
                    if (afterhit == -1)
                    {
                        return;
                    }

                    afterhit += hittype.Length;
                    var forname = " for ";
                    var nameofthing = message.IndexOf(forname);
                    if (nameofthing == -1)
                    {
                        return;
                    }
                    var nameofthinglength = nameofthing - afterhit;
                    var nameofthethingforreal = message.Substring(afterhit, nameofthinglength);

                    Debug.WriteLine($"'{nameofchar}' '{nameofthethingforreal}' '{damagedone}'");
                    TryAdd(nameofthethingforreal, int.Parse(damagedone), nameofchar);
                }
                else if (LastTimeFighting.HasValue && (now - LastTimeFighting.Value).TotalSeconds > 20)
                {
                    LastTimeFighting = null;
                    CurrentEntityList.Clear();
                }
            });
        }

        private void TryAdd(string nameoftarget, int damagedone, string nameofdealer)
        {
            var item = CurrentEntityList.FirstOrDefault(a => a.Name == nameofdealer);
            if (item == null)
            {
                CurrentEntityList.Add(new EntittyDPS
                {
                    Name = nameofdealer,
                    TotalDamage = damagedone,
                    TotalSeconds = 0
                });
            }
            else
            {
                item.TotalDamage += damagedone;
            }
        }
    }
}
