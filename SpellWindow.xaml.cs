using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;

namespace EQTool
{
    public partial class SpellWindow : Window
    {
        public ObservableCollection<UISpell> SpellList = new ObservableCollection<UISpell>();
        private readonly Timer ParseTimer;
        private readonly Timer UITimer;
        private long? LastReadOffset;
        private readonly string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";
        private readonly string YouBeginCasting = "you begin casting ";
        private readonly List<Spell> AllSpells = new List<Spell>();
        private readonly Dictionary<string, Spell> CastOtherSpells = new Dictionary<string, Spell>();
        private readonly Dictionary<string, Spell> CastOnYouSpells = new Dictionary<string, Spell>();
        private readonly Dictionary<string, Spell> YouCastSpells = new Dictionary<string, Spell>();
        private Spell UserCastingSpell;
        private int UserCastingSpellCounter = 0;
        private int? Level = 1;

        public SpellWindow()
        {
            InitializeComponent();
            var spells = ParseSpells.GetSpells();
            var spellicons = SpellIcons.GetSpellIcons();

            foreach (var item in spells)
            {
                var mappedspell = item.Map(spellicons);
                AllSpells.Add(mappedspell);
                if (mappedspell.buffduration > 0)
                {
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_other))
                    {
                        if (CastOtherSpells.TryGetValue(mappedspell.cast_on_other, out var innerval))
                        {
                            if (innerval.Level < mappedspell.Level)
                            {
                                CastOtherSpells[mappedspell.cast_on_other] = mappedspell;
                            }
                        }
                        else
                        {
                            CastOtherSpells.Add(mappedspell.cast_on_other, mappedspell);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.name) && mappedspell.Level > 0)
                    {
                        if (!YouCastSpells.ContainsKey(mappedspell.name))
                        {
                            YouCastSpells.Add(mappedspell.name, mappedspell);
                        }
                        else
                        {
                            Debug.WriteLine($"Skipping spell: {mappedspell.name}");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_you) && mappedspell.Level > 0)
                    {
                        if (!CastOnYouSpells.ContainsKey(mappedspell.cast_on_you))
                        {
                            CastOnYouSpells.Add(mappedspell.cast_on_you, mappedspell);
                        }
                        else
                        {
                            Debug.WriteLine($"Skipping cast_on_you spell: {mappedspell.cast_on_you}");
                        }
                    }
                }
            }

            PollUpdates(null, null);

            ParseTimer = new System.Timers.Timer(500);
            ParseTimer.Elapsed += PollUpdates;
            ParseTimer.Enabled = true;

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;

            spelllistview.ItemsSource = SpellList;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            var groupDescription = new PropertyGroupDescription("TargetName");
            view.GroupDescriptions.Add(groupDescription);

            var players = Properties.Settings.Default.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var directory = new DirectoryInfo(Models.EqToolSettings.BestGuessRootEqPath + "/Logs/");
            var loggedincharlogfile = directory.GetFiles()
                .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                .OrderByDescending(a => a.LastWriteTime)
                .FirstOrDefault();
            if (loggedincharlogfile != null)
            {
                var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                var indexpart = charname.IndexOf("_");
                var charName = charname.Substring(0, indexpart);
                Level = players.FirstOrDefault(a => a.Name == charName)?.Level;
            }
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
                foreach (var item in SpellList)
                {
                    item.SecondsLeftOnSpell = TimeSpan.FromSeconds(item.SecondsLeftOnSpell.TotalSeconds - 1);
                    if (item.SecondsLeftOnSpell.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                }

                foreach (var item in itemstoremove)
                {
                    _ = SpellList.Remove(item);
                }
            });
        }

        private void PollUpdates(object sender, EventArgs e)
        {
            var directory = new DirectoryInfo(Models.EqToolSettings.BestGuessRootEqPath + "/Logs/");
            var loggedincharlogfile = directory.GetFiles()
                .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                .OrderByDescending(a => a.LastWriteTime)
                .FirstOrDefault();
            if (loggedincharlogfile == null)
            {
                return;
            }

            if (!LastReadOffset.HasValue || LastReadOffset >= loggedincharlogfile.Length)
            {
                LastReadOffset = loggedincharlogfile.Length;
            }

            using (var stream = new FileStream(loggedincharlogfile.FullName, FileMode.Open, FileAccess.Read))
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

        private void ParseLine(string line)
        {
            var date = line.Substring(1, 25);
            if (DateTime.TryParse(date, out var parseddate))
            {

            }

            var message = line.Substring(27).ToLower();
            Debug.WriteLine(message);
            if (message.StartsWith(YouBeginCasting))
            {
                var spellname = message.Substring(YouBeginCasting.Length - 1).Trim().TrimEnd('.');
                if (YouCastSpells.TryGetValue(spellname, out var foundspell))
                {
                    Debug.WriteLine($"Self Casting Spell: {spellname} Delay: {foundspell.casttime}");
                    UserCastingSpell = foundspell;
                    if (!Level.HasValue)
                    {
                        Level = UserCastingSpell.Level;
                    }
                    if (UserCastingSpell.casttime > 0)
                    {
                        var oldreference = UserCastingSpell;
                        UserCastingSpellCounter++;
                        _ = Task.Delay(UserCastingSpell.casttime + 1500).ContinueWith(a =>
                        {
                            Debug.WriteLine($"Cleaning Spell");
                            if (--UserCastingSpellCounter <= 0)
                            {
                                UserCastingSpellCounter = 0;
                                UserCastingSpell = null;
                            }
                        });
                    }
                }
            }

            if (UserCastingSpell != null)
            {
                if (message == UserCastingSpell.cast_on_you)
                {
                    Debug.WriteLine($"Self Finished Spell: {message}");
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        TryAdd(UserCastingSpell, "You");
                        UserCastingSpell = null;
                    });
                }
                else if (message.EndsWith(UserCastingSpell.cast_on_other))
                {
                    var targetname = message.Replace(UserCastingSpell.cast_on_other, string.Empty).Trim();
                    Debug.WriteLine($"Self Finished Spell: {message}");
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        TryAdd(UserCastingSpell, targetname);
                        UserCastingSpell = null;
                    });
                }
            }
        }

        private void TryAdd(Spell spell, string target)
        {
            if (spell == null)
            {
                return;
            }

            var s = SpellList.FirstOrDefault(a => a.SpellName == spell.name && a.TargetName == target);
            if (s != null)
            {
                _ = SpellList.Remove(s);
            }

            SpellList.Add(new UISpell
            {
                TargetName = target,
                SpellName = UserCastingSpell.name,
                Rect = UserCastingSpell.Rect,
                SecondsLeftOnSpell = TimeSpan.FromSeconds(GetDuration_inSeconds(UserCastingSpell)),
                SpellIcon = UserCastingSpell.SpellIcon
            }); ;
        }

        private int GetDuration_inSeconds(Spell spell)
        {
            _ = spell.buffdurationformula;
            var duration = spell.buffduration;
            int spell_ticks;
            switch (spell.buffdurationformula)
            {
                case 0:
                    spell_ticks = 0;
                    break;
                case 1:
                    spell_ticks = (int)Math.Ceiling(Level.Value / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 2:
                    spell_ticks = (int)Math.Ceiling(Level.Value / 5.0f * 3);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 3:
                    spell_ticks = Level.Value * 30;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 4:
                    spell_ticks = duration == 0 ? 50 : duration;
                    break;
                case 5:
                    spell_ticks = duration;
                    if (spell_ticks == 0)
                    {
                        spell_ticks = 3;
                    }

                    break;
                case 6:
                    spell_ticks = (int)Math.Ceiling(Level.Value / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 7:
                    spell_ticks = Level.Value;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 8:
                    spell_ticks = Level.Value + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 9:
                    spell_ticks = (Level.Value * 2) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 10:
                    spell_ticks = (Level.Value * 3) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 11:
                case 12:
                case 15:
                    spell_ticks = duration;
                    break;
                case 50:
                    spell_ticks = 72000;
                    break;
                case 3600:
                    spell_ticks = duration == 0 ? 3600 : duration;
                    break;
                default:
                    spell_ticks = duration;
                    break;
            }

            return spell_ticks * 6;
        }
    }
}
