using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class SpellWindowViewModel : INotifyPropertyChanged
    {
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private readonly EQSpells spells;

        public SpellWindowViewModel(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQToolSettings settings, EQSpells spells)
        {

            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            Title = "Triggers v" + App.Version;
            this.spells = spells;
        }


        public ObservableCollection<PersistentViewModel> _SpellList = new ObservableCollection<PersistentViewModel>();
        public ObservableCollection<PersistentViewModel> SpellList
        {
            get => _SpellList;
            set
            {
                _SpellList = value;
                OnPropertyChanged();
            }
        }

        private string _Title = null;
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }

        public void UpdateStuff()
        {
            OnPropertyChanged(nameof(SpellList));
        }

        private long? _LastReadOffset = null;
        public long? LastReadOffset
        {
            get => _LastReadOffset;
            set
            {
                _LastReadOffset = value;
                OnPropertyChanged();
            }
        }

        public void ClearYouSpells()
        {
            var itemstoremove = SpellList.Where(a => a.GroupName == EQSpells.SpaceYou).ToList();
            foreach (var item in itemstoremove)
            {
                _ = SpellList.Remove(item);
            }
        }

        public void UpdateSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                var player = activePlayer.Player;
                var itemstoremove = new List<PersistentViewModel>();
                var timerTypes = new List<SpellViewModelType>() { SpellViewModelType.Roll, SpellViewModelType.Spell, SpellViewModelType.Timer };
                foreach (var item in SpellList.Where(a => timerTypes.Contains(a.SpellViewModelType)).Cast<TimerViewModel>().ToList())
                {
                    item.TotalRemainingDuration = item.TotalRemainingDuration.Subtract(TimeSpan.FromSeconds(1));
                    if (item.TotalRemainingDuration.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                }
                foreach (var item in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).Cast<SpellViewModel>().ToList())
                {
                    item.HideGuesses = !settings.BestGuessSpells;
                    item.ShowOnlyYou = settings.YouOnlySpells;
                    item.HideClasses = player != null && SpellUIExtensions.HideSpell(player.ShowSpellsForClasses, item.Classes) && item.GroupName != EQSpells.SpaceYou;
                    if (item.SpellType == SpellTypes.RandomRoll)
                    {
                        item.HideClasses = !settings.ShowRandomRolls;
                    }
                }
                var d = DateTime.Now;
                var persistentTypes = new List<SpellViewModelType>() { SpellViewModelType.Persistent, SpellViewModelType.Counter };
                foreach (var item in SpellList.Where(a => persistentTypes.Contains(a.SpellViewModelType)).Cast<PersistentViewModel>().ToList())
                {
                    if ((d - item.UpdatedDateTime).TotalMinutes > 20)
                    {
                        itemstoremove.Add(item);
                    }
                }

                var groupedspells = SpellList.GroupBy(a => a.GroupName).ToList();
                foreach (var spells in groupedspells)
                {
                    var allspellshidden = true;
                    foreach (var spell in spells)
                    {
                        if (spell.ColumnVisibility != System.Windows.Visibility.Collapsed)
                        {
                            allspellshidden = false;
                        }
                    }

                    if (allspellshidden)
                    {
                        foreach (var spell in spells)
                        {
                            spell.HeaderVisibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        foreach (var spell in spells)
                        {
                            spell.HeaderVisibility = System.Windows.Visibility.Visible;
                        }
                    }
                }

                foreach (var item in itemstoremove)
                {
                    _ = SpellList.Remove(item);
                }
            });
        }

        public void ClearAllSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                while (SpellList.Count > 0)
                {
                    SpellList.RemoveAt(SpellList.Count - 1);
                }
            });
        }

        private readonly List<string> SpellsThatNeedCounts = new List<string>()
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
        };

        private readonly List<string> SpellsThatDragonsDo = new List<string>()
        {
            "Dragon Roar",
            "Silver Breath",
            "Ice breath",
            "Mind Cloud",
            "Rotting Flesh",
            "Putrefy Flesh",
            "Stun Breath",
            "Immolating Breath",
            "Rain of Molten Lava",
            "Frost Breath",
            "Lava Breath",
            "Cloud of Fear",
            "Diseased Cloud",
            "Tsunami",
            "Ancient Breath"
        };

        public void TryAdd(SpellCastEvent match, bool resisted)
        {
            if (match?.Spell == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spellname = match.Spell.name;
                if (string.Equals(match.Spell.name, "Harvest", StringComparison.OrdinalIgnoreCase) && match.TargetName == EQSpells.SpaceYou)
                {
                    TryAddCustom(new CustomTimer
                    {
                        DurationInSeconds = 600,
                        Name = "--CoolDown-- " + spellname,
                        SpellNameIcon = spellname,
                        SpellType = SpellTypes.HarvestCooldown
                    });
                    return;
                }
                if (SpellsThatDragonsDo.Contains(match.Spell.name))
                {
                    TryAddCustom(new CustomTimer
                    {
                        DurationInSeconds = (int)(match.Spell.recastTime / 1000.0),
                        Name = "--CoolDown-- " + spellname,
                        SpellNameIcon = spellname,
                        SpellType = SpellTypes.BadGuyCoolDown
                    });
                    return;
                }

                if (match.Spell.name.EndsWith("Discipline"))
                {
                    var basetime = (int)(match.Spell.recastTime / 1000.0);
                    var playerlevel = activePlayer.Player.Level;
                    if (match.Spell.name == "Evasive Discipline")
                    {
                        float baseseconds = 15 * 60;
                        float levelrange = 60 - 51;
                        float secondsrange = (15 - 7) * 60;
                        var secondsperlevelrange = secondsrange / levelrange;
                        float playerleveltick = playerlevel - 52;
                        basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                    }
                    else if (match.Spell.name == "Defensive Discipline")
                    {
                        float baseseconds = 15 * 60;
                        float levelrange = 60 - 54;
                        float secondsrange = (15 - 10) * 60;
                        var secondsperlevelrange = secondsrange / levelrange;
                        float playerleveltick = playerlevel - 55;
                        basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                    }
                    else if (match.Spell.name == "Precision Discipline")
                    {
                        float baseseconds = 30 * 60;
                        float levelrange = 60 - 56;
                        float secondsrange = (30 - 27) * 60;
                        var secondsperlevelrange = secondsrange / levelrange;
                        float playerleveltick = playerlevel - 57;
                        basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                    }
                    TryAddCustom(new CustomTimer
                    {
                        DurationInSeconds = basetime,
                        Name = "--Discipline-- " + spellname,
                        SpellNameIcon = "Strengthen",
                        SpellType = SpellTypes.DisciplineCoolDown,
                        TargetName = match.TargetName,
                        Classes = match.Spell.Classes
                    });
                }
                if (resisted)
                {
                    return;
                }
                var needscount = SpellsThatNeedCounts.Contains(spellname);
                if (needscount)
                {
                    var countervm = new CounterViewModel
                    {
                        Count = 1,
                        Icon = match.Spell.SpellIcon,
                        GroupName = match.TargetName,
                        Name = spellname,
                        Rect = match.Spell.Rect,
                        UpdatedDateTime = DateTime.Now
                    };
                    if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == spellname && match.TargetName == a.GroupName) is CounterViewModel s)
                    {
                        s.Count += 1;
                        s.UpdatedDateTime = DateTime.Now;
                    }
                    else
                    {
                        SpellList.Add(countervm);
                    }
                }
                else
                {
                    var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match.Spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                    var duration = needscount ? 0 : match.TotalSecondsOverride ?? spellduration.TotalSeconds;
                    var isnpc = MasterNPCList.NPCs.Contains(match.TargetName);
                    var uispell = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        SpellType = match.Spell.type,
                        GroupName = match.TargetName,
                        Name = spellname,
                        Rect = match.Spell.Rect,
                        Icon = match.Spell.SpellIcon,
                        Classes = match.Spell.Classes,
                        GuessedSpell = match.MultipleMatchesFound,
                        IsNPC = isnpc,
                        TotalDuration = spellduration,
                        TotalRemainingDuration = spellduration
                    };

                    SpellList.Add(uispell);
                }
            });
        }

        public void TryAddCustom(CustomTimer match)
        {
            if (match?.Name == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var s = SpellList.FirstOrDefault(a => a.Name == match.Name && match.TargetName == a.GroupName);
                if (s != null)
                {
                    if (match.SpellType != SpellTypes.RandomRoll)
                    {
                        _ = SpellList.Remove(s);
                    }
                }

                var spellduration = match.DurationInSeconds;
                var spellicon = spells.AllSpells.FirstOrDefault(a => a.name == match.SpellNameIcon);
                if (match.SpellType == SpellTypes.RandomRoll)
                {
                    var rollsingroup = SpellList.Where(a => a.Name == match.TargetName && a.SpellViewModelType == SpellViewModelType.Roll).Cast<RollViewModel>().ToList();
                    foreach (var item in rollsingroup)
                    {
                        //reset the timer on all of the rolls
                        item.TotalRemainingDuration = TimeSpan.FromSeconds(match.DurationInSeconds);
                    }
                    SpellList.Add(new RollViewModel
                    {
                        PercentLeft = 100,
                        GroupName = match.TargetName,
                        Name = match.Name,
                        Rect = spellicon.Rect,
                        Icon = spellicon.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(match.DurationInSeconds),
                        TotalRemainingDuration = TimeSpan.FromSeconds(match.DurationInSeconds),
                        Roll = match.Roll,
                        UpdatedDateTime = DateTime.Now
                    });
                }
                else
                {
                    SpellList.Add(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = match.TargetName,
                        Name = match.Name,
                        Rect = spellicon.Rect,
                        Icon = spellicon.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(match.DurationInSeconds),
                        TotalRemainingDuration = TimeSpan.FromSeconds(match.DurationInSeconds),
                        UpdatedDateTime = DateTime.Now
                    });
                }
            });
        }

        public void AddSavedYouSpells(List<YouSpells> youspells)
        {
            if (youspells == null || !youspells.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                foreach (var item in youspells)
                {
                    var match = spells.AllSpells.FirstOrDefault(a => a.name == item.Name);
                    if (match != null)
                    {
                        var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                        var savedspellduration = item.TotalSecondsLeft;
                        //var uispell = new UISpell(DateTime.Now.AddSeconds(savedspellduration), false)
                        //{
                        //    UpdatedDateTime = DateTime.Now,
                        //    PercentLeftOnSpell = 100,
                        //    SpellType = match.type,
                        //    TargetName = EQSpells.SpaceYou,
                        //    SpellName = match.name,
                        //    Rect = match.Rect,
                        //    PersistentSpell = false,
                        //    Counter = null,
                        //    SpellIcon = match.SpellIcon,
                        //    Classes = match.Classes,
                        //    GuessedSpell = false
                        //};
                        //SpellList.Add(uispell);
                    }
                }
            });
        }

        public void TryRemoveUnambiguousSpellOther(string possiblespell)
        {
            if (string.IsNullOrWhiteSpace(possiblespell))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var s = SpellList.Where(a => a.Name == possiblespell && a.GroupName != EQSpells.SpaceYou).ToList();
                if (s.Count() == 1)
                {
                    _ = SpellList.Remove(s.FirstOrDefault());
                }
            });
        }

        public void TryRemoveUnambiguousSpellSelf(List<string> possiblespellnames)
        {
            if (!possiblespellnames.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList.Where(a => possiblespellnames.Contains(a.Name) && a.GroupName == EQSpells.SpaceYou).ToList();
                if (spells.Count() == 1)
                {
                    _ = SpellList.Remove(spells.FirstOrDefault());
                }
            });
        }

        public void TryRemoveCustom(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var s = SpellList.FirstOrDefault(a => a.Name == name && CustomTimer.CustomerTime == a.GroupName);
                if (s != null)
                {
                    _ = SpellList.Remove(s);
                }
            });
        }

        public void TryRemoveTarget(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spellstormove = SpellList.Where(a => string.Equals(a.Name, target, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var item in spellstormove)
                {
                    Debug.WriteLine($"Removing {item.Name}");
                    _ = SpellList.Remove(item);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
