using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static EQTool.Services.Spells.Log.LogCustomTimer;

namespace EQTool.ViewModels
{
    public class SpellWindowViewModel : INotifyPropertyChanged
    {
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private readonly EQSpells spells;
        private readonly string CustomerTime = " Custom Timer";
        private readonly Dictionary<PlayerClasses, int> CustomTimerClasses;

        public SpellWindowViewModel(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQToolSettings settings, EQSpells spells)
        {
            CustomTimerClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().Select(a => new { key = a, level = 1 }).ToDictionary(a => a.key, a => a.level);
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            Title = "Triggers v" + App.Version;
            FeignDeath = spells.AllSpells.FirstOrDefault(a => a.name == "Feign Death");
            this.spells = spells;
        }


        private Spell FeignDeath { get; set; }

        public ObservableCollection<UISpell> _SpellList = new ObservableCollection<UISpell>();
        public ObservableCollection<UISpell> SpellList
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
            var itemstoremove = SpellList.Where(a => a.TargetName == EQSpells.SpaceYou).ToList();
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
                var itemstoremove = new List<UISpell>();

                var d = DateTime.Now;
                foreach (var item in SpellList)
                {
                    item.UpdateTimeLeft();
                    if (item.SecondsLeftOnSpell.TotalSeconds <= 0 && !item.PersistentSpell)
                    {
                        itemstoremove.Add(item);
                    }
                    else if (item.PersistentSpell && (d - item.UpdatedDateTime).TotalMinutes > 5)
                    {
                        itemstoremove.Add(item);
                    }
                    item.HideGuesses = !settings.BestGuessSpells;
                    item.ShowOnlyYou = settings.YouOnlySpells;
                    item.HideClasses = player != null && SpellUIExtensions.HideSpell(player.ShowSpellsForClasses, item.Classes) && item.TargetName != EQSpells.SpaceYou;
                }

                var groupedspells = SpellList.GroupBy(a => a.TargetName).ToList();
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

        public void TryAdd(SpellParsingMatch match)
        {
            if (match?.Spell == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spellname = match.Spell.name;
                if (match.MultipleMatchesFound)
                {
                    spellname = "??? " + spellname;
                }

                var ismanasieve = spellname == "Mana Sieve";
                var ispersistent = ismanasieve;
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match.Spell, activePlayer.Player));
                var duraction = match.TotalSecondsOverride ?? spellduration.TotalSeconds;
                var uispell = new UISpell(DateTime.Now.AddSeconds((int)duraction))
                {
                    UpdatedDateTime = DateTime.Now,
                    PercentLeftOnSpell = 100,
                    SpellType = match.Spell.type,
                    TargetName = match.TargetName,
                    SpellName = spellname,
                    Rect = match.Spell.Rect,
                    PersistentSpell = ispersistent,
                    SieveCounter = ismanasieve ? 1 : (int?)null,
                    SpellIcon = match.Spell.SpellIcon,
                    Classes = match.Spell.Classes,
                    GuessedSpell = match.MultipleMatchesFound
                };
                var s = SpellList.FirstOrDefault(a => a.SpellName == spellname && match.TargetName == a.TargetName);
                if (s != null)
                {
                    if (ismanasieve)
                    {
                        s.SieveCounter += 1;
                        s.UpdatedDateTime = DateTime.Now;
                    }
                    else
                    {
                        _ = SpellList.Remove(s);
                        SpellList.Add(uispell);
                    }
                }
                else
                {
                    SpellList.Add(uispell);
                }
            });
        }

        public void TryAddCustom(CustomerTimer match)
        {
            if (match?.Name == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var s = SpellList.FirstOrDefault(a => a.SpellName == match.Name && CustomerTime == a.TargetName);
                if (s != null)
                {
                    _ = SpellList.Remove(s);
                }

                var spellduration = match.DurationInSeconds;
                SpellList.Add(new UISpell(DateTime.Now.AddSeconds(spellduration))
                {
                    UpdatedDateTime = DateTime.Now,
                    PercentLeftOnSpell = 100,
                    SpellType = -1,
                    TargetName = CustomerTime,
                    SpellName = match.Name,
                    Rect = FeignDeath.Rect,
                    SpellIcon = FeignDeath.SpellIcon,
                    Classes = CustomTimerClasses,
                    GuessedSpell = false
                });
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
                    var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match, activePlayer.Player));
                    var savedspellduration = item.TotalSecondsLeft;
                    var uispell = new UISpell(DateTime.Now.AddSeconds(savedspellduration))
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeftOnSpell = 100,
                        SpellType = match.type,
                        TargetName = EQSpells.SpaceYou,
                        SpellName = match.name,
                        Rect = match.Rect,
                        PersistentSpell = false,
                        SieveCounter = null,
                        SpellIcon = match.SpellIcon,
                        Classes = match.Classes,
                        GuessedSpell = false
                    };
                    SpellList.Add(uispell);
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
                var s = SpellList.Where(a => a.SpellName == possiblespell && a.TargetName != EQSpells.SpaceYou).ToList();
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
                var spells = SpellList.Where(a => possiblespellnames.Contains(a.SpellName) && a.TargetName == EQSpells.SpaceYou).ToList();
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
                var s = SpellList.FirstOrDefault(a => a.SpellName == name && CustomerTime == a.TargetName);
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
                var spellstormove = SpellList.Where(a => a.TargetName.ToLower() == target.ToLower()).ToList();
                foreach (var item in spellstormove)
                {
                    Debug.WriteLine($"Removing {item.SpellName}");
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
