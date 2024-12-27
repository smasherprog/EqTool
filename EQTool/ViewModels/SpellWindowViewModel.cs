using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
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
            this.spells = spells;

            this.appDispatcher.DispatchUI(() =>
            {
                Title = "Triggers v" + App.Version;
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
            });
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

        private LinearGradientBrush NonRaidModeLinearGradientBrush;
        private LinearGradientBrush RaidModeLinearGradientBrush;
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
                if (_RaidModeEnabled == value)
                {
                    return;
                }
                _RaidModeEnabled = value;
                if (_RaidModeEnabled)
                {
                    RaidModeButtonToolTip = "Disable Raid Mode";
                    WindowFrameBrush = RaidModeLinearGradientBrush;
                }
                else
                {
                    RaidModeButtonToolTip = "Enable Raid Mode";
                    WindowFrameBrush = NonRaidModeLinearGradientBrush;
                }
                OnPropertyChanged(nameof(RaidModeButtonToolTip));
                OnPropertyChanged();
            }
        }

        private int _SpellGroupCount = 0;
        private int SpellGroupCount
        {
            get => _SpellGroupCount;
            set
            {
                _SpellGroupCount = value;
                if (_SpellGroupCount > 10)
                {
                    RaidModeToggleButtonVisibility = Visibility.Visible;
                }
                else
                {
                    RaidModeToggleButtonVisibility = Visibility.Collapsed;
                }
            }
        }

        public string RaidModeButtonToolTip { get; set; } = "Disable Raid Mode";

        private Visibility _RaidModeToggleButtonVisibility = Visibility.Collapsed;

        public Visibility RaidModeToggleButtonVisibility
        {
            get => _RaidModeToggleButtonVisibility;
            set
            {
                if (_RaidModeToggleButtonVisibility == value)
                {
                    return;
                }
                _RaidModeToggleButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public void UpdateSpells(double dt_ms)
        {
            appDispatcher.DispatchUI(() =>
            {
                var player = activePlayer.Player;
                var raidmodedetection = settings.RaidModeDetection ?? true;
                SpellGroupCount = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).GroupBy(a => a.GroupName).Count();
                RaidModeEnabled = raidmodedetection && player?.PlayerClass != null && SpellGroupCount > 10;
                var itemstoremove = new List<PersistentViewModel>();
                var timerTypes = new List<SpellViewModelType>() { SpellViewModelType.Roll, SpellViewModelType.Spell, SpellViewModelType.Timer };
                foreach (var item in SpellList.Where(a => timerTypes.Contains(a.SpellViewModelType)).Cast<TimerViewModel>().ToList())
                {
                    var hidespell = false;
                    if (item.GroupName != CustomTimer.CustomerTime || !item.TargetClass.HasValue)
                    {
                        if (item.SpellViewModelType == SpellViewModelType.Timer)
                        {
                            if (!MasterNPCList.NPCs.Contains(item.GroupName.Trim()))
                            {
                                if (settings.YouOnlySpells)
                                {
                                    hidespell = !(item.GroupName == CustomTimer.CustomerTime || item.GroupName == EQSpells.SpaceYou);
                                }
                                else if (RaidModeEnabled && item.GroupName != EQSpells.SpaceYou)
                                {
                                    hidespell = true;
                                }
                            }
                        }
                    }
                    item.TotalRemainingDuration = item.TotalRemainingDuration.Subtract(TimeSpan.FromMilliseconds(dt_ms));
                    if (item.TotalRemainingDuration.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                    item.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }
                var spells = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).Cast<SpellViewModel>().ToList();

                foreach (var item in spells)
                {
                    var hidespell = false;
                    if (!MasterNPCList.NPCs.Contains(item.GroupName.Trim()))
                    {
                        if (settings.YouOnlySpells)
                        {
                            hidespell = !(item.GroupName == CustomTimer.CustomerTime || item.GroupName == EQSpells.SpaceYou);
                        }
                        else if (RaidModeEnabled && player.PlayerClass.HasValue)
                        {
                            if (item.GroupName != EQSpells.SpaceYou)
                            {
                                hidespell = SpellUIExtensions.HideSpell(new List<EQToolShared.Enums.PlayerClasses>() { player.PlayerClass.Value }, item.Classes) || item.SpellType == SpellType.Self;
                            }
                        }
                        else
                        {
                            hidespell = SpellUIExtensions.HideSpell(player.ShowSpellsForClasses, item.Classes) && item.GroupName != EQSpells.SpaceYou;
                        }
                    }

                    item.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }
                var d = DateTime.Now;
                var persistentTypes = new List<SpellViewModelType>() { SpellViewModelType.Persistent, SpellViewModelType.Counter };
                foreach (var item in SpellList.Where(a => persistentTypes.Contains(a.SpellViewModelType)).Cast<PersistentViewModel>().ToList())
                {
                    var hidespell = false;
                    if (settings.YouOnlySpells)
                    {
                        hidespell = !(MasterNPCList.NPCs.Contains(item.GroupName.Trim()) || item.GroupName == CustomTimer.CustomerTime || item.GroupName == EQSpells.SpaceYou);
                    }
                    if ((d - item.UpdatedDateTime).TotalMinutes > 20)
                    {
                        itemstoremove.Add(item);
                    }
                    item.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }

                var groupedspellList = SpellList.GroupBy(a => a.GroupName).ToList();
                foreach (var triggers in groupedspellList)
                {
                    var allspellshidden = true;
                    foreach (var spell in triggers)
                    {
                        if (spell.ColumnVisibility != System.Windows.Visibility.Collapsed)
                        {
                            allspellshidden = false;
                        }
                    }

                    if (allspellshidden)
                    {
                        foreach (var spell in triggers)
                        {
                            spell.HeaderVisibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        foreach (var spell in triggers)
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

        public void ClearAllOtherSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                var spellstoremove = SpellList
                .Where(a => a.SpellViewModelType == SpellViewModelType.Spell)
                .Cast<SpellViewModel>()
                .Where(a => a.GroupName != EQSpells.SpaceYou)
                .ToList();

                foreach (var spell in spellstoremove)
                {
                    if (!MasterNPCList.NPCs.Contains(spell.GroupName.Trim()))
                    {
                        _ = SpellList.Remove(spell);
                    }
                }
            });
        }

        // try to add a new timer.  set overWrite = true (default) to overwrite/refresh any existing timer, or false to create multiple timers of same name
        public void TryAdd(SpellViewModel match, bool overWrite = true)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (overWrite)
                {
                    if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell &&
                    string.Equals(a.Name, match.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(match.GroupName, a.GroupName, StringComparison.OrdinalIgnoreCase)) is SpellViewModel s)
                    {
                        _ = SpellList.Remove(s);
                    }
                }
                SpellList.Add(match);
            });
        }

        public void TryAdd(CounterViewModel match)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter &&
                string.Equals(a.Name, match.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(match.GroupName, a.GroupName, StringComparison.OrdinalIgnoreCase)) is CounterViewModel s)
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
                var rollsingroup = SpellList.Where(a => string.Equals(match.GroupName, a.GroupName, StringComparison.OrdinalIgnoreCase) && a.SpellViewModelType == SpellViewModelType.Roll).Cast<RollViewModel>().ToList();
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
                var existing = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer &&
                string.Equals(a.Name, match.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(match.GroupName, a.GroupName, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    _ = SpellList.Remove(existing);
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
                    var match = spells.AllSpells.FirstOrDefault(a => string.Equals(a.name, item.Name, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                        var savedspellduration = item.TotalSecondsLeft;
                        var uispell = new SpellViewModel
                        {
                            UpdatedDateTime = DateTime.Now,
                            PercentLeft = 100,
                            BenefitDetriment = match.benefit_detriment,
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
                var s = SpellList.Where(a => string.Equals(a.Name, possiblespell, StringComparison.OrdinalIgnoreCase) && a.GroupName != EQSpells.SpaceYou).ToList();
                if (s.Count() == 1)
                {
                    _ = SpellList.Remove(s.FirstOrDefault());
                }
            });
        }

        public void TryRemoveUnambiguousSpellOther(List<string> possiblespellnames)
        {
            if (!possiblespellnames.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList.Where(a => possiblespellnames.Any(b => string.Equals(a.Name, b, StringComparison.OrdinalIgnoreCase))).ToList();
                if (spells.Count() == 1)
                {
                    _ = SpellList.Remove(spells.FirstOrDefault());
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
                var spells = SpellList.Where(a => possiblespellnames.Any(b => string.Equals(a.Name, b, StringComparison.OrdinalIgnoreCase)) && a.GroupName == EQSpells.SpaceYou).ToList();
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
