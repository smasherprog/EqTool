using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells;
using EQTool.ViewModels.MobInfoComponents;
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
        private readonly PigParseApi pigParseApi;
        private readonly BoatScheduleService boatScheduleService;
        private readonly PetViewModel playerPet;
        private bool PCSpellsGroupedByTarget = false;
        private bool NPCSpellsGroupedByTarget = false;

        public SpellWindowViewModel(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQToolSettings settings, EQSpells spells, BoatScheduleService boatScheduleService, PigParseApi pigParseApi, PetViewModel playerPet)
        {
            this.activePlayer = activePlayer;
            this.pigParseApi = pigParseApi;
            this.boatScheduleService = boatScheduleService;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            this.playerPet = playerPet;
            this.spells = spells;
            Title = "Triggers v" + App.Version;
        }

        private ObservableCollection<BaseTriggerViewModel> _SpellList;
        public ObservableCollection<BaseTriggerViewModel> SpellList
        {
            get
            {
                CreateSpellList();
                return _SpellList;
            }
        }

        private void CreateSpellList()
        {
            if (_SpellList == null)
            {
                NPCSpellsGroupedByTarget = PCSpellsGroupedByTarget = false;
                _SpellList = new ObservableCollection<BaseTriggerViewModel>();
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_SpellList);
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TimerViewModel.GroupName)));
                view.LiveGroupingProperties.Add(nameof(TimerViewModel.GroupName));
                view.IsLiveGrouping = true;
                view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.Sorting), ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription(nameof(RollViewModel.Roll), ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.TotalRemainingDuration), ListSortDirection.Ascending));
                view.IsLiveSorting = true;
                view.LiveSortingProperties.Add(nameof(TimerViewModel.TotalRemainingDuration));
                foreach (var item in Zones.Boats)
                {
                    var boatcolor = Brushes.Aquamarine;
                    if (item.Boat == Boats.BloatedBelly)
                    {
                        boatcolor = Brushes.DarkSalmon;
                    }
                    else if (item.Boat == Boats.BarrelBarge)
                    {
                        boatcolor = Brushes.CadetBlue;
                    }
                    else if (item.Boat == Boats.NroIcecladBoat)
                    {
                        boatcolor = Brushes.DeepPink;
                    }
                    else if (item.Boat == Boats.MaidensVoyage)
                    {
                        boatcolor = Brushes.DarkGoldenrod;
                    }
                    _SpellList.Add(new BoatViewModel
                    {
                        Name = item.PrettyName,
                        Boat = item,
                        TotalDuration = TimeSpan.FromSeconds(item.TripTimeInSeconds),
                        ProgressBarColor = boatcolor,
                    });
                }
                OnPropertyChanged(nameof(SpellList));
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

        private LinearGradientBrush _WindowFrameBrush = null;

        public LinearGradientBrush WindowFrameBrush
        {
            get
            {
                if (_WindowFrameBrush == null)
                {
                    _WindowFrameBrush = new LinearGradientBrush
                    {
                        StartPoint = new System.Windows.Point(0, 0.5),
                        EndPoint = new System.Windows.Point(1, 0.5),
                        GradientStops = new GradientStopCollection()
                    {
                            new GradientStop(System.Windows.Media.Colors.CadetBlue, .4),
                            new GradientStop(System.Windows.Media.Colors.Gray, 1)
                    }
                    };
                }

                return _WindowFrameBrush;
            }
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
                    WindowFrameBrush = new LinearGradientBrush
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
                else
                {
                    RaidModeButtonToolTip = "Enable Raid Mode";
                    WindowFrameBrush = new LinearGradientBrush
                    {
                        StartPoint = new System.Windows.Point(0, 0.5),
                        EndPoint = new System.Windows.Point(1, 0.5),
                        GradientStops = new GradientStopCollection()
                        {
                                new GradientStop(System.Windows.Media.Colors.CadetBlue, .4),
                                new GradientStop(System.Windows.Media.Colors.Gray, 1)
                        }
                    };
                }
                OnPropertyChanged(nameof(RaidModeButtonToolTip));
                OnPropertyChanged();
            }
        }


        private bool _RaidModeDetection = true;
        public bool RaidModeDetection
        {
            get => _RaidModeDetection;
            set
            {
                _RaidModeDetection = value;
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
                SpellGroupCount = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).GroupBy(a => a.GroupName).Count();
                RaidModeEnabled = RaidModeDetection && player?.PlayerClass != null && SpellGroupCount > 10;
                var itemstoremove = new List<BaseTriggerViewModel>();

                //run all timers remaining duration down, and remove any that are expired
                var timerTypes = new List<SpellViewModelType>() { SpellViewModelType.Roll, SpellViewModelType.Spell, SpellViewModelType.Timer };
                foreach (var item in SpellList.Where(a => timerTypes.Contains(a.SpellViewModelType)).Cast<TimerViewModel>().ToList())
                {
                    item.TotalRemainingDuration = item.TotalRemainingDuration.Subtract(TimeSpan.FromMilliseconds(dt_ms));
                    if (item.TotalRemainingDuration.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                }

                //update rolls
                var randomRollsVisible = settings.ShowRandomRolls == false ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                foreach (var item in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Roll).Cast<TimerViewModel>().ToList())
                {
                    item.ColumnVisibility = randomRollsVisible;
                }

                //update spells
                foreach (var s in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).Cast<SpellViewModel>())
                {
                    var isNPCTarget = !s.IsTargetPlayer;
                    if (s.GroupName == EQSpells.SpaceYou)
                    {
                        s.ColumnVisibility = System.Windows.Visibility.Visible;
                        continue;
                    }
                    var hidespell = false;
                    if (settings.YouOnlySpells && s.GroupName != CustomTimer.CustomerTime && !isNPCTarget)
                    {
                        hidespell = true;
                    }
                    else if (RaidModeEnabled && player.PlayerClass.HasValue)
                    {
                        hidespell = SpellUIExtensions.HideSpell(new List<EQToolShared.Enums.PlayerClasses>() { player.PlayerClass.Value }, s.Classes) || s.SpellType == SpellType.Self;
                    }
                    else
                    {
                        hidespell = SpellUIExtensions.HideSpell(player.ShowSpellsForClasses, s.Classes);
                    }

                    s.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }

                foreach (var item in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer).Cast<TimerViewModel>().ToList())
                {
                    var hidespell = false;
                    if (!item.GroupName.StartsWith(" ") && (settings.YouOnlySpells || RaidModeEnabled))
                    {
                        hidespell = true;
                    }

                    if (item.Name.StartsWith(CustomTimer.ScoutTime))
                    {
                        hidespell = settings.ShowScoutRollTime == false;
                    }
                    else if (item.Name.StartsWith(CustomTimer.Ring8))
                    {
                        hidespell = settings.ShowRing8RollTime == false;
                    }

                    item.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }

                var boats = _SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Boat).Cast<BoatViewModel>().ToList();
                foreach (var boat in boats)
                {
                    if (BoatScheduleService.SupportdBoats.Contains(boat.Boat.Boat))
                    {
                        boat.TotalRemainingDuration = boat.TotalRemainingDuration.Subtract(TimeSpan.FromMilliseconds(dt_ms));
                        if (boat.TotalRemainingDuration.TotalSeconds <= 0)
                        {
                            var dt = TimeSpan.FromMilliseconds(boat.TotalRemainingDuration.Milliseconds);
                            boat.TotalRemainingDuration = dt.Add(boat.TotalDuration);
                        }
                    }

                    if (player?.BoatSchedule == false)
                    {
                        boat.ColumnVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        boat.ColumnVisibility = Visibility.Visible;
                    }
                }

                var d = DateTime.Now;
                //these are for spells like mana seive which are a counter, or flame lick/ war epic procs which are persistent but we still want to hide them after a while if they aren't refreshed
                foreach (var item in SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Counter).Cast<CounterViewModel>())
                {
                    var hidespell = false;
                    if (settings.YouOnlySpells)
                    {
                        hidespell = !(!item.IsTargetPlayer || item.GroupName == CustomTimer.CustomerTime || item.GroupName == EQSpells.SpaceYou);
                    }
                    if ((d - item.UpdatedDateTime).TotalMinutes > 10)
                    {
                        itemstoremove.Add(item);
                    }
                    item.ColumnVisibility = hidespell ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }

                var spellsQuery = SpellList.Where(a =>
                a.SpellViewModelType == SpellViewModelType.Spell &&
                a.GroupName != EQSpells.SpaceYou &&
                a.GroupName != playerPet.PetName &&
                a.IsTargetPlayer).Cast<SpellViewModel>().ToList();

                var spellsByGroupName = spellsQuery.GroupBy(a => a.GroupName).ToList();
                var spellsByName = spellsQuery.GroupBy(a => a.Name).ToList();

                if (spellsByGroupName.Count > spellsByName.Count)
                {
                    foreach (var spell in spellsQuery)
                    {
                        (spell.Name, spell.GroupName) = (spell.GroupName, spell.Name);
                    }
                    PCSpellsGroupedByTarget = !PCSpellsGroupedByTarget;
                }

                spellsQuery = SpellList.Where(a =>
                a.SpellViewModelType == SpellViewModelType.Spell &&
                !a.IsTargetPlayer &&
                a.GroupName != playerPet.PetName &&
                a.IsTargetPlayer).Cast<SpellViewModel>().ToList();

                spellsByGroupName = spellsQuery.GroupBy(a => a.GroupName).ToList();
                spellsByName = spellsQuery.GroupBy(a => a.Name).ToList();
                if (spellsByGroupName.Count > spellsByName.Count)
                {
                    foreach (var spell in spellsQuery)
                    {
                        (spell.Name, spell.GroupName) = (spell.GroupName, spell.Name);
                    }
                    NPCSpellsGroupedByTarget = !NPCSpellsGroupedByTarget;
                }

                var groupedspellList = SpellList.GroupBy(a => a.GroupName).ToList();
                foreach (var triggers in groupedspellList)
                {
                    var allspellshidden = triggers.All(a => a.ColumnVisibility == System.Windows.Visibility.Collapsed);
                    var headerVisibility = allspellshidden ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    foreach (var spell in triggers)
                    {
                        spell.HeaderVisibility = headerVisibility;
                    }

                    var groupname = triggers.FirstOrDefault()?.GroupName ?? string.Empty;
                    if (playerPet.PetName == groupname)
                    {
                        foreach (var spell in triggers)
                        {
                            spell.HeaderVisibility = System.Windows.Visibility.Visible;
                            spell.ColumnVisibility = System.Windows.Visibility.Visible;
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
                PCSpellsGroupedByTarget = false;
                var spellstoremove = SpellList
                .Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.IsTargetPlayer)
                .Cast<SpellViewModel>()
                .Where(a => a.GroupName != EQSpells.SpaceYou)
                .ToList();

                foreach (var spell in spellstoremove)
                {
                    _ = SpellList.Remove(spell);
                }
            });
        }

        // try to add a new timer.  set overWrite = true (default) to overwrite/refresh any existing timer, or false to create multiple timers of same name
        public void TryAdd(SpellViewModel match, bool overWrite = true)
        {
            appDispatcher.DispatchUI(() =>
            {
                var spellname = match.Name;
                var groupname = match.GroupName;
                if ((match.IsTargetPlayer && PCSpellsGroupedByTarget) || (!match.IsTargetPlayer && NPCSpellsGroupedByTarget && match.Name != EQSpells.SpaceYou))
                {
                    match.GroupName = spellname;
                    match.Name = groupname;
                }

                if (overWrite)
                {
                    if (SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell &&
                    string.Equals(match.Name, a.Name, StringComparison.OrdinalIgnoreCase) &&
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

        public void TryAdd(TimerViewModel match, bool allowDuplicates = false)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (!allowDuplicates)
                {
                    var existing = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer &&
                     string.Equals(a.Name, match.Name, StringComparison.OrdinalIgnoreCase) &&
                     string.Equals(match.GroupName, a.GroupName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        _ = SpellList.Remove(existing);
                    }
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
                    if (spells.AllSpells.TryGetValue(item.Name, out var match))
                    {
                        var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                        var savedspellduration = item.TotalSecondsLeft;
                        var uispell = new SpellViewModel
                        {
                            IsTargetPlayer = true,
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

        private Spell RollTimerIcon = null;
        public void UpdateAPITimers()
        {
            var s = activePlayer.Player.Server;
            if (s.HasValue)
            {
                var boatsapi = pigParseApi.GetBoatData(s.Value);
                appDispatcher.DispatchUI(() =>
                {
                    var boats = _SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Boat).Cast<BoatViewModel>().ToList();
                    foreach (var boat in boatsapi)
                    {
                        boatScheduleService.UpdateBoatInformation(boat, boats, DateTimeOffset.Now);
                    }
                });
                var timersData = pigParseApi.GetRollTimers(s.Value);
                appDispatcher.DispatchUI(() =>
                {
                    var existing = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer && (a.Name.StartsWith("Ring 8 Roll Timer") || a.Name.StartsWith("Scout Charisa Timer"))).ToList();
                    foreach (var item in existing)
                    {
                        _ = SpellList.Remove(item);
                    }

                    if (RollTimerIcon == null)
                    {
                        _ = spells.AllSpells.TryGetValue("Feign Death", out var feignDeath);
                        RollTimerIcon = feignDeath;
                    }

                    var timers = timersData.Where(a => a.RollTimerType == EQToolShared.APIModels.RollTimerType.Scout).ToList();

                    if (timers.Any())
                    {
                        var timer = timers.Where(a => !a.Guess).OrderByDescending(a => a.DateTime).FirstOrDefault();
                        if (timer == null)
                        {
                            timer = timers.Where(a => a.Guess).OrderByDescending(a => a.DateTime).FirstOrDefault();
                        }

                        var match = new TimerViewModel
                        {
                            PercentLeft = 100,
                            GroupName = CustomTimer.CustomerTime,
                            Name = "Ring 8 Roll Timer" + (timer.Guess ? " (Guess)" : ""),
                            Rect = RollTimerIcon.Rect,
                            Icon = RollTimerIcon.SpellIcon,
                            TotalDuration = TimeSpan.FromHours(10),
                            TotalRemainingDuration = TimeSpan.FromSeconds(10),
                            UpdatedDateTime = DateTime.Now,
                            ProgressBarColor = Brushes.LightGreen
                        };

                        match.Name = "Scout Charisa Timer";
                        match.TotalDuration = TimeSpan.FromHours(10);
                        if (timer.DateTime > DateTimeOffset.Now)
                        {
                            match.TotalRemainingDuration = timer.DateTime - DateTimeOffset.Now;
                        }
                        else
                        {
                            match.TotalRemainingDuration = TimeSpan.FromHours(10);
                            match.Name = $"Scout Charisa Timer (UNKNOWN)";
                        }
                        SpellList.Add(match);
                    }

                    timers = timersData.Where(a => a.RollTimerType == EQToolShared.APIModels.RollTimerType.Quake).ToList();

                    if (timers.Any())
                    {
                        var timer = timers.Where(a => !a.Guess).OrderByDescending(a => a.DateTime).FirstOrDefault();
                        var match = new TimerViewModel
                        {
                            PercentLeft = 100,
                            GroupName = CustomTimer.CustomerTime,
                            Name = "Ring 8 Roll Timer" + (timer.Guess ? " (Guess)" : ""),
                            Rect = RollTimerIcon.Rect,
                            Icon = RollTimerIcon.SpellIcon,
                            TotalDuration = TimeSpan.FromHours(10),
                            TotalRemainingDuration = TimeSpan.FromSeconds(10),
                            UpdatedDateTime = DateTime.Now,
                            ProgressBarColor = Brushes.LightGreen
                        };

                        match.Name = "Ring 8 Roll Timer";
                        match.TotalDuration = TimeSpan.FromHours(24);
                        while (timer.DateTime < DateTimeOffset.Now)
                        {
                            timer.DateTime = timer.DateTime.AddHours(24);
                            if (timer.DateTime > DateTimeOffset.Now)
                            {
                                match.TotalRemainingDuration = timer.DateTime - DateTimeOffset.Now;
                                if (match.TotalRemainingDuration.TotalMinutes >= 30)
                                {
                                    match.TotalRemainingDuration -= TimeSpan.FromMinutes(30);//ring roll always 30 minutes before quake
                                }
                                else
                                {
                                    match.TotalRemainingDuration = TimeSpan.FromHours(24).Subtract(TimeSpan.FromMinutes(30 - match.TotalRemainingDuration.Minutes));

                                }
                            }
                        }
                        SpellList.Add(match);
                    }
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
