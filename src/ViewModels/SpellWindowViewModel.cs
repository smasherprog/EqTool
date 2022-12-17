using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
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
        private readonly string CustomerTime = " Custom Timer";
        private readonly Dictionary<PlayerClasses, int> CustomTimerClasses;

        public SpellWindowViewModel(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQToolSettings settings, EQSpells spells)
        {
            CustomTimerClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().Select(a => new { key = a, level = 1 }).ToDictionary(a => a.key, a => a.level);
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            FeignDeath = spells.AllSpells.FirstOrDefault(a => a.name == "Feign Death");
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

        public void UpdateSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                var player = activePlayer.Player;
                var itemstoremove = new List<UISpell>();

                foreach (var item in SpellList)
                {
                    item.SecondsLeftOnSpell = TimeSpan.FromSeconds(item.SecondsLeftOnSpell.TotalSeconds - 1);
                    if (item.SecondsLeftOnSpell.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                    item.HideGuesses = !settings.BestGuessSpells;
                    item.ShowOnlyYou = settings.YouOnlySpells;
                    item.HideClasses = player != null && SpellUIExtentions.HideSpell(player.ShowSpellsForClasses, item.Classes) && item.TargetName != EQSpells.SpaceYou;
                }

                foreach (var spells in SpellList.GroupBy(a => a.TargetName))
                {
                    var allspellshidden = true;
                    foreach (var spell in spells)
                    {
                        if (spell.ColumnVisiblity != System.Windows.Visibility.Collapsed)
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
                if (match.MutipleMatchesFound)
                {
                    spellname = "??? " + spellname;
                }

                var s = SpellList.FirstOrDefault(a => a.SpellName == spellname && match.TargetName == a.TargetName);
                if (s != null)
                {
                    _ = SpellList.Remove(s);
                }

                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match.Spell, activePlayer.Player));
                SpellList.Add(new UISpell
                {
                    TotalSecondsOnSpell = (int)spellduration.TotalSeconds,
                    PercentLeftOnSpell = 100,
                    SpellType = match.Spell.type,
                    TargetName = match.TargetName,
                    SpellName = spellname,
                    Rect = match.Spell.Rect,
                    SecondsLeftOnSpell = spellduration,
                    SpellIcon = match.Spell.SpellIcon,
                    Classes = match.Spell.Classes,
                    GuessedSpell = match.MutipleMatchesFound
                });
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
                SpellList.Add(new UISpell
                {
                    TotalSecondsOnSpell = spellduration,
                    PercentLeftOnSpell = 100,
                    SpellType = -1,
                    TargetName = CustomerTime,
                    SpellName = match.Name,
                    Rect = FeignDeath.Rect,
                    SecondsLeftOnSpell = TimeSpan.FromSeconds(spellduration),
                    SpellIcon = FeignDeath.SpellIcon,
                    Classes = CustomTimerClasses,
                    GuessedSpell = false
                });
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
