using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;

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
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(SpellList);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TimerViewModel.GroupName)));
            view.LiveGroupingProperties.Add(nameof(TimerViewModel.GroupName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.Sorting), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(RollViewModel.Roll), ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.TotalRemainingDuration), ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(TimerViewModel.TotalRemainingDuration));

            _WindowFrameBrush = NonRaidModeLinearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0.5),
                EndPoint = new System.Windows.Point(1, 0.5),
                GradientStops = new GradientStopCollection()
            {
                new GradientStop(System.Windows.Media.Colors.CadetBlue, .4),
                new GradientStop(System.Windows.Media.Colors.Gray, 1)
            }
            };
            RaidModeLinearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0.5),
                EndPoint = new System.Windows.Point(1, 0.5),
                GradientStops = new GradientStopCollection()
            {
                new GradientStop(System.Windows.Media.Colors.OrangeRed, .4),
                new GradientStop(System.Windows.Media.Colors.Gray, 1)
            }
            };
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

        public void ClearYouSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                var itemstoremove = SpellList.Where(a => a.GroupName == EQSpells.SpaceYou).ToList();
                foreach (var item in itemstoremove)
                {
                    _ = SpellList.Remove(item);
                }
            });
        }

        private readonly LinearGradientBrush NonRaidModeLinearGradientBrush;
        private readonly LinearGradientBrush RaidModeLinearGradientBrush;

        private LinearGradientBrush _WindowFrameBrush;

        public LinearGradientBrush WindowFrameBrush
        {
            get => _WindowFrameBrush;
            set
            {
                _WindowFrameBrush = value;
                OnPropertyChanged();
            }
        }
        private bool _RaidModeEnabled = false;
        public bool RaidModeEnabled
        {
            get => _RaidModeEnabled;
            set
            {
                _RaidModeEnabled = value;
                if (_RaidModeEnabled)
                {
                    WindowFrameBrush = RaidModeLinearGradientBrush;
                }
                else
                {
                    WindowFrameBrush = NonRaidModeLinearGradientBrush;
                }
                OnPropertyChanged();
            }
        }

        public void UpdateSpells(double dt_ms)
        {
            appDispatcher.DispatchUI(() =>
            {
                var player = activePlayer.Player;
                var raidmodedetection = settings.RaidModeDetection ?? true;
                var groupcount = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).GroupBy(a => a.GroupName).Count();
                RaidModeEnabled = raidmodedetection && player?.PlayerClass != null && groupcount > 10;
                var itemstoremove = new List<PersistentViewModel>();
                var timerTypes = new List<SpellViewModelType>() { SpellViewModelType.Roll, SpellViewModelType.Spell, SpellViewModelType.Timer };
                foreach (var item in SpellList.Where(a => timerTypes.Contains(a.SpellViewModelType)).Cast<TimerViewModel>().ToList())
                {
                    item.TotalRemainingDuration = item.TotalRemainingDuration.Subtract(TimeSpan.FromMilliseconds(dt_ms));
                    if (item.TotalRemainingDuration.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                }
                foreach (var item in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).Cast<SpellViewModel>().ToList())
                {
                    item.ShowOnlyYou = settings.YouOnlySpells;
                    if (RaidModeEnabled && player.PlayerClass.HasValue)
                    {
                        if (item.GroupName != EQSpells.SpaceYou)
                        {
                            item.HideClasses = SpellUIExtensions.HideSpell(new List<EQToolShared.Enums.PlayerClasses>() { player.PlayerClass.Value }, item.Classes) && item.GroupName != EQSpells.SpaceYou;
                            if (item.SpellType == SpellType.Self)
                            {
                                item.HideClasses = true;
                            }
                        }
                    }
                    else
                    {
                        item.HideClasses = player != null && SpellUIExtensions.HideSpell(player.ShowSpellsForClasses, item.Classes) && item.GroupName != EQSpells.SpaceYou;
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

        public void TryAdd(SpellViewModel match)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == match.Name && match.GroupName == a.GroupName) is SpellViewModel s)
                {
                    _ = SpellList.Remove(s);
                }
                SpellList.Add(match);
            });
        }

        public void TryAdd(CounterViewModel match)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == match.Name && match.GroupName == a.GroupName) is CounterViewModel s)
                {
                    s.Count += 1;
                    s.UpdatedDateTime = DateTime.Now;
                }
                else
                {
                    SpellList.Add(match);
                }
            });
        }

        public void TryAdd(RollViewModel match)
        {
            appDispatcher.DispatchUI(() =>
            {
                var rollsingroup = SpellList.Where(a => a.GroupName == match.GroupName && a.SpellViewModelType == SpellViewModelType.Roll).Cast<RollViewModel>().ToList();
                foreach (var item in rollsingroup)
                {
                    //reset the timer on all of the rolls
                    item.TotalRemainingDuration = TimeSpan.FromTicks(match.TotalDuration.Ticks);
                }
                SpellList.Add(match);
            });
        }

        public void TryAdd(TimerViewModel match)
        {
            appDispatcher.DispatchUI(() =>
            {
                var existing = SpellList.FirstOrDefault(a => a.Name == match.Name && a.SpellViewModelType == SpellViewModelType.Timer && a.GroupName == match.GroupName);
                if(existing != null)
                {
                    SpellList.Remove(existing);
                }
                SpellList.Add(match);
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
                        var uispell = new SpellViewModel
                        {
                            UpdatedDateTime = DateTime.Now,
                            PercentLeft = 100,
                            Type = match.type,
                            SpellType = match.SpellType,
                            GroupName = EQSpells.SpaceYou,
                            Name = match.name,
                            Rect = match.Rect,
                            Icon = match.SpellIcon,
                            Classes = match.Classes,
                            TotalDuration = spellduration,
                            TotalRemainingDuration = TimeSpan.FromSeconds(savedspellduration)
                        };
                        SpellList.Add(uispell);
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
