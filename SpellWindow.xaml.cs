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
using System.Windows.Input;

namespace EQTool
{
    public partial class SpellWindow : Window
    {
        public ObservableCollection<UISpell> SpellList = new ObservableCollection<UISpell>();
        private readonly Timer ParseTimer;
        private readonly Timer UITimer;
        private long? LastReadOffset;
        private readonly string ZoneLoadingMessage = "LOADING, PLEASE WAIT...";
        private readonly string YouBeginCasting = "You begin casting ";
        private readonly string Your = "Your ";
        private readonly string You = "You ";
        private readonly List<Spell> AllSpells = new List<Spell>();
        private readonly Dictionary<string, List<Spell>> CastOtherSpells = new Dictionary<string, List<Spell>>();
        private readonly Dictionary<string, List<Spell>> CastOnYouSpells = new Dictionary<string, List<Spell>>();
        private readonly Dictionary<string, List<Spell>> YouCastSpells = new Dictionary<string, List<Spell>>();
        private Spell UserCastingSpell;
        private int UserCastingSpellCounter = 0;
        private PlayerInfo LastPlayerFound = null;
        private readonly List<string> IgnoreSpellsList = new List<string>()
        {
            "Complete Heal",
            "Denon`s Disruptive Discord",
            "Chords of Dissonance"
        };

        private readonly EQToolSettings settings;

        public SpellWindow(EQToolSettings settings, ParseSpells parseSpells, SpellIcons spellIcons)
        {
            this.settings = settings;
            InitializeComponent();
            _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(delegate (object sender, ExecutedRoutedEventArgs args) { Close(); })));
            var spells = parseSpells.GetSpells().Where(a => !IgnoreSpellsList.Contains(a.name) && a.spell_icon > 0);
            var spellicons = spellIcons.GetSpellIcons();
            Topmost = settings.TriggerWindowTopMost;
            foreach (var item in spells)
            {
                var mappedspell = item.Map(spellicons);
                AllSpells.Add(mappedspell);
                if (mappedspell.buffduration > 0)
                {
                    //Debug.WriteLine($"'{mappedspell.name}'---'{mappedspell.cast_on_you}'---'{mappedspell.cast_on_other}'");
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_other))
                    {
                        if (CastOtherSpells.TryGetValue(mappedspell.cast_on_other, out var innerval))
                        {
                            CastOtherSpells[mappedspell.cast_on_other].Add(mappedspell);
                        }
                        else
                        {
                            CastOtherSpells.Add(mappedspell.cast_on_other, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.name) && mappedspell.Level > 0)
                    {
                        if (YouCastSpells.TryGetValue(mappedspell.name, out var innerval))
                        {
                            YouCastSpells[mappedspell.name].Add(mappedspell);
                        }
                        else
                        {
                            YouCastSpells.Add(mappedspell.name, new List<Spell>() { mappedspell });
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(mappedspell.cast_on_you))
                    {
                        if (CastOnYouSpells.TryGetValue(mappedspell.cast_on_you, out var innerval))
                        {
                            CastOnYouSpells[mappedspell.cast_on_you].Add(mappedspell);
                        }
                        else
                        {
                            CastOnYouSpells.Add(mappedspell.cast_on_you, new List<Spell>() { mappedspell });
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Spell {mappedspell.name} Ignored");
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
            _ = TryUpdatePlayerLevel();
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
            if (loggedincharlogfile != null)
            {
                var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                var indexpart = charname.IndexOf("_");
                var charName = charname.Substring(0, indexpart);
                var tempplayer = players.FirstOrDefault(a => a.Name == charName);
                if (tempplayer != LastPlayerFound)
                {
                    SpellList.Clear();
                    LastReadOffset = null;
                }
                LastPlayerFound = tempplayer;
            }

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

        private Spell GetClosestmatch(List<Spell> spells)
        {
            var level = LastPlayerFound?.Level;
            if (!level.HasValue)
            {
                return spells.FirstOrDefault(a => a.Level > 0 && a.Level <= 60);
            }

            Spell closestspell = null;
            var leveldelta = 100;
            foreach (var item in spells)
            {
                var delta = Math.Abs(item.Level - level.Value);
                if (delta < leveldelta)
                {
                    leveldelta = delta;
                    closestspell = item;
                }
            }

            return closestspell;
        }

        private void ParseLine(string line)
        {
            var date = line.Substring(1, 25);
            if (DateTime.TryParse(date, out var parseddate))
            {

            }

            var message = line.Substring(27);
            Debug.WriteLine(message);
            if (message.StartsWith(YouBeginCasting))
            {
                var spellname = message.Substring(YouBeginCasting.Length - 1).Trim().TrimEnd('.');
                if (YouCastSpells.TryGetValue(spellname, out var foundspells))
                {
                    var foundspell = GetClosestmatch(foundspells);
                    Debug.WriteLine($"Self Casting Spell: {spellname} Delay: {foundspell.casttime}");
                    UserCastingSpell = foundspell;

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
            else if (message.StartsWith(You))
            {
                if (CastOnYouSpells.TryGetValue(message, out var foundspells))
                {
                    var foundspell = GetClosestmatch(foundspells);
                    Debug.WriteLine($"Your Casting Spell: {message} Delay: {foundspell.casttime}");
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        TryAdd(foundspell, "You");
                    });
                }
            }

            if (message.StartsWith(Your))
            {
                var spellname = message.Substring(Your.Length - 1).Trim().TrimEnd('.');
                if (CastOnYouSpells.TryGetValue(spellname, out var foundspells))
                {
                    var foundspell = GetClosestmatch(foundspells);
                    Debug.WriteLine($"Your Casting Spell: {spellname} Delay: {foundspell.casttime}");
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        TryAdd(foundspell, "You");
                    });
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
                else if (!string.IsNullOrWhiteSpace(UserCastingSpell.cast_on_other) && message.EndsWith(UserCastingSpell.cast_on_other))
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
            else if (settings.BestGuessSpells)
            {
                var removename = message.IndexOf("'");
                if (removename != -1)
                {
                    var spellmessage = message.Substring(removename).Trim();
                    if (CastOtherSpells.TryGetValue(spellmessage, out var foundspells))
                    {
                        var foundspell = GetClosestmatch(foundspells);
                        var targetname = message.Replace(foundspell.cast_on_other, string.Empty).Trim();
                        Debug.WriteLine($"Other Spell: {foundspell.name} Message: {spellmessage}");
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            TryAdd(foundspell, targetname);
                        });
                    }
                }
                else
                {
                    removename = message.IndexOf(" ");
                    if (removename != -1)
                    {
                        var spellmessage = message.Substring(removename).Trim();
                        if (CastOtherSpells.TryGetValue(spellmessage, out var foundspells))
                        {
                            var foundspell = GetClosestmatch(foundspells);
                            var targetname = message.Replace(foundspell.cast_on_other, string.Empty).Trim();
                            Debug.WriteLine($"Other Spell: {foundspell.name} Message: {spellmessage}");
                            App.Current.Dispatcher.Invoke(delegate
                            {
                                TryAdd(foundspell, targetname);
                            });
                        }
                    }
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
            _ = TryUpdatePlayerLevel();
            var level = LastPlayerFound?.Level;
            if (!level.HasValue)
            {
                level = UserCastingSpell.Level;
            }
            var spellduration = TimeSpan.FromSeconds(GetDuration_inSeconds(spell, level));
            SpellList.Add(new UISpell
            {
                TotalSecondsOnSpell = (int)spellduration.TotalSeconds,
                PercentLeftOnSpell = 100,
                SpellType = spell.type,
                TargetName = target,
                SpellName = spell.name,
                Rect = spell.Rect,
                SecondsLeftOnSpell = spellduration,
                SpellIcon = spell.SpellIcon
            });
        }

        private static int GetDuration_inSeconds(Spell spell, int? userlevel)
        {
            _ = spell.buffdurationformula;
            var duration = spell.buffduration;
            int spell_ticks;
            var level = userlevel.HasValue ? (spell.Level > userlevel.Value ? spell.Level : userlevel.Value) : spell.Level;
            switch (spell.buffdurationformula)
            {
                case 0:
                    spell_ticks = 0;
                    break;
                case 1:
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 2:
                    spell_ticks = (int)Math.Ceiling(level / 5.0f * 3);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 3:
                    spell_ticks = level * 30;
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
                    spell_ticks = (int)Math.Ceiling(level / 2.0f);
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 7:
                    spell_ticks = level;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 8:
                    spell_ticks = level + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 9:
                    spell_ticks = (level * 2) + 10;
                    spell_ticks = Math.Min(spell_ticks, duration);
                    break;
                case 10:
                    spell_ticks = (level * 3) + 10;
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
