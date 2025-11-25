using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels.MobInfoComponents;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.APIModels.PlayerControllerModels;
using EQToolShared.Enums;
using EQToolShared.Extensions;

namespace EQTool.ViewModels
{
    public class SpellWindowViewModel : BaseWindowViewModel, INotifyPropertyChanged
    {
        private const int minimumTargetsForRaidMode = 10;
        
        private readonly SpellGroupingEngine engine;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private readonly EQSpells spells;
        private readonly PigParseApi pigParseApi;
        private readonly BoatScheduleService boatScheduleService;
        private readonly PetViewModel playerPet;
        private HashSet<SpellViewModel> recentlyImpactedSpells = new HashSet<SpellViewModel>();
        
        internal int groupRevaluationDebounceTime { get; set; } = 1000;  // Debounce delay for re-evaluating the ByTarget / BySpell Grouping of the list. It's internal so that tests can change it for speed.

        public SpellWindowViewModel(
            SpellGroupingEngine engine,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
            EQSpells spells,
            BoatScheduleService boatScheduleService,
            PigParseApi pigParseApi,
            PetViewModel playerPet)
        {
            this.engine = engine;
            this.activePlayer = activePlayer;
            this.pigParseApi = pigParseApi;
            this.boatScheduleService = boatScheduleService;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            this.playerPet = playerPet;
            this.spells = spells;
            Title = "Triggers v" + App.Version;
            settings.PropertyChanged += Base_PropertyChanged;
            PropertyChanged += Base_PropertyChanged;
        }
        
        private ObservableCollection<PersistentViewModel> _SpellList;
        public ObservableCollection<PersistentViewModel> SpellList
        {
            get
            {
                CreateTriggerList();
                return _SpellList;
            }
            set
            {
                _SpellList = value;
                OnPropertyChanged();
            }
        }
        
        private void CreateTriggerList()
        {
            if (_SpellList == null)
            {
                _SpellList = new ObservableCollection<PersistentViewModel>();
                SpellList.CollectionChanged += SpellList_CollectionChanged;
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_SpellList);
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TimerViewModel.DisplayGroup)));
                view.LiveGroupingProperties.Add(nameof(TimerViewModel.DisplayGroup));
                view.IsLiveGrouping = true;
                view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.GroupSorting), ListSortDirection.Ascending));
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
                        Id = item.PrettyName,
                        Boat = item,
                        TotalDuration = TimeSpan.FromSeconds(item.TripTimeInSeconds),
                        ProgressBarColor = boatcolor,
                    });
                }
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
                var itemsToRemove = SpellList.Where(s => s is SpellViewModel vm && vm.CastOnYou()).ToList();
                foreach (var item in itemsToRemove)
                    _ = SpellList.Remove(item);
            });
        }
        
        public void ClearYouSpellsExceptPersistentCooldowns()
        {
            appDispatcher.DispatchUI(() =>
            {
                var nonDisciplinCooldowns = SpellList
                    .Where(s => s is SpellViewModel vm
                        && vm.CastOnYou()
                        && !(vm.BenefitDetriment == SpellBenefitDetriment.Cooldown && s.Id.Contains("Discipline", StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                foreach (var item in nonDisciplinCooldowns)
                    _ = SpellList.Remove(item);
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
                        StartPoint = new Point(0, 0.5),
                        EndPoint = new Point(1, 0.5),
                        GradientStops = new GradientStopCollection()
                    {
                            new GradientStop(Colors.CadetBlue, .4),
                            new GradientStop(Colors.Gray, 1)
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
                        StartPoint = new Point(0, 0.5),
                        EndPoint = new Point(1, 0.5),
                        GradientStops = new GradientStopCollection()
                        {
                            new GradientStop(Colors.OrangeRed, .4),
                            new GradientStop(Colors.Gray, 1)
                        }
                    };
                }
                else
                {
                    RaidModeButtonToolTip = "Enable Raid Mode";
                    WindowFrameBrush = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0.5),
                        EndPoint = new Point(1, 0.5),
                        GradientStops = new GradientStopCollection()
                        {
                            new GradientStop(Colors.CadetBlue, .4),
                            new GradientStop(Colors.Gray, 1)
                        }
                    };
                }
                QueueFullSpellListReevaluationIfNecessary();
                OnPropertyChanged(nameof(RaidModeButtonToolTip));
                OnPropertyChanged();
            }
        }

        private int _SpellTargetCount = 0;
        private int SpellTargetCount
        {
            get => _SpellTargetCount;
            set
            {
                _SpellTargetCount = value;
                if (_SpellTargetCount > minimumTargetsForRaidMode)
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
            get => IsCurrentlyClickThrough ? Visibility.Hidden : _RaidModeToggleButtonVisibility;
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

        public Visibility GenericButtonVisibility => IsCurrentlyClickThrough ? Visibility.Hidden : Visibility.Visible;

        public void UpdateTriggers(double dt_ms)
        {
            appDispatcher.DispatchUI(() =>
            {
                var player = activePlayer.Player;
                var raidModeDetection = settings.RaidModeDetection ?? true;
                SpellTargetCount = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).GroupBy(a => a.Target).Count();
                RaidModeEnabled = raidModeDetection && player?.PlayerClass != null && SpellTargetCount > minimumTargetsForRaidMode;
               
                var itemsToRemove = new List<PersistentViewModel>();
                UpdateSpellsAndTimers(dt_ms, itemsToRemove);
                UpdateBoats(dt_ms);
                UpdatePersistentCounters(itemsToRemove);
                UpdateGlobalVisibility();

                foreach (var item in itemsToRemove)
                {
                    _ = SpellList.Remove(item);
                }
            });
        }

        private void UpdateSpellsAndTimers(double dt_ms, List<PersistentViewModel> itemsToRemove)
        {
            var timerTypes = new List<SpellViewModelType> { SpellViewModelType.Roll, SpellViewModelType.Spell, SpellViewModelType.Timer };
            
            foreach (var item in SpellList.Where(a => timerTypes.Contains(a.SpellViewModelType)).Cast<TimerViewModel>().ToList())
            {
                item.TotalRemainingDuration = item.TotalRemainingDuration.Subtract(TimeSpan.FromMilliseconds(dt_ms));
                if (item.TotalRemainingDuration.TotalSeconds <= 0)
                {
                    itemsToRemove.Add(item);
                }

                var hideTrigger = false;
                if (item is SpellViewModel spell)
                {
                    hideTrigger = ShouldFilterSpell(spell, activePlayer.Player);
                }
                else if (item.SpellViewModelType == SpellViewModelType.Timer)
                {
                    //TODO: Make misc custom timers their own setting as well
                    if (item.Target == CustomTimer.CustomerTime)
                    {
                        if (item.Id.StartsWith(CustomTimer.ScoutTime))
                        {
                            hideTrigger = false;
                            if (settings.ShowScoutRollTime == false)
                            {
                                hideTrigger = true;
                            }
                        }
                        if (item.Id.StartsWith(CustomTimer.Ring8))
                        {
                            hideTrigger = false;
                            if (settings.ShowRing8RollTime == false)
                            {
                                hideTrigger = true;
                            }
                        }
                    }
                }

                item.ColumnVisibility = hideTrigger ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void UpdateBoats(double dt_ms)
        {
            var boats = _SpellList.OfType<BoatViewModel>().ToList();
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

                if (activePlayer.Player?.BoatSchedule == false)
                {
                    boat.ColumnVisibility = Visibility.Collapsed;
                }
                else
                {
                    boat.ColumnVisibility = Visibility.Visible;
                }
            }
        }

        private void UpdatePersistentCounters(List<PersistentViewModel> itemsToRemove)
        {
            var d = DateTime.Now;
            var persistentTypes = new List<SpellViewModelType> { SpellViewModelType.Persistent, SpellViewModelType.Counter };
            foreach (var item in SpellList.Where(a => persistentTypes.Contains(a.SpellViewModelType)).ToList())
            {
                var hideTrigger = false;
                if (item is CounterViewModel counter)
                {
                    hideTrigger = ShouldFilterSpell(counter, activePlayer.Player);
                }

                if ((d - item.UpdatedDateTime).TotalMinutes > 20)
                {
                    itemsToRemove.Add(item);
                }
                item.ColumnVisibility = hideTrigger ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void UpdateGlobalVisibility()
        {
            var groupedTriggerList = SpellList.GroupBy(a => a.DisplayGroup).ToList();
            foreach (var triggers in groupedTriggerList)
            {
                var allTriggersHidden = true;
                foreach (var trigger in triggers)
                {
                    if (trigger.ColumnVisibility != Visibility.Collapsed)
                    {
                        allTriggersHidden = false;
                    }
                }

                if (allTriggersHidden)
                {
                    foreach (var trigger in triggers)
                    {
                        trigger.HeaderVisibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    foreach (var trigger in triggers)
                    {
                        trigger.HeaderVisibility = Visibility.Visible;
                    }
                }

                //TODO: Verify if this needs some fine-tuning with the new Id/Target/DisplayName/DisplayGroup system. It looks a little incompatible but I don't really understand its purpose
                var groupName = triggers.FirstOrDefault()?.DisplayGroup ?? string.Empty;
                if (playerPet.PetName == groupName)
                {
                    foreach (var trigger in triggers)
                    {
                        trigger.HeaderVisibility = Visibility.Visible;
                        trigger.ColumnVisibility = Visibility.Visible;
                    }
                }
            }
        }

        private bool ShouldFilterSpell(SpellViewModel s, PlayerInfo player)
        {
            if (RaidModeEnabled)
            {
                if (s.Target == CustomTimer.CustomerTime || s.CastByYou() || s.CastOnYou() || s.CastByYourClass(player))
                {
                    return false;
                }
                
                if (MasterNPCList.NPCs.Contains(s.Target.Trim()))
                {
                    // Detrimental spells and cooldowns on raid targets should always be shown in raid mode
                    if (s.BenefitDetriment == SpellBenefitDetriment.Cooldown || s.BenefitDetriment == SpellBenefitDetriment.Detrimental)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            
            switch (settings.SpellsFilter)
            {
                case SpellsFilterType.CastOnYou when !s.CastOnYou():
                case SpellsFilterType.CastByYou when !s.CastByYou():
                case SpellsFilterType.CastByOrOnYou when !(s.CastOnYou() || s.CastByYou()):
                    return true;
                case SpellsFilterType.ByClass:
                    return !IsClassSpellAllowed(s.Classes, player.ShowSpellsForClasses);
            }

            return false;
        }

        private static bool IsClassSpellAllowed(Dictionary<PlayerClasses, int> spellClasses, IEnumerable<PlayerClasses> allowedClasses)
        {
            if (allowedClasses == null || spellClasses == null || spellClasses.Count == 0)
            {
                return true;
            }

            return allowedClasses.Any(spellClasses.ContainsKey);
        }

        public void ClearSpellsNotCastOnYou()
        {
            appDispatcher.DispatchUI(() =>
            {
                var spellsToRemove = SpellList
                    .OfType<SpellViewModel>()
                    .Where(a => !a.CastOnYou())
                    .ToList();
                
                foreach (var spell in spellsToRemove)
                {
                    if (!MasterNPCList.NPCs.Contains(spell.Target.Trim()))
                    {
                        _ = SpellList.Remove(spell);
                    }
                }
            });
        }
        
        public void ClearSpellsCastByOthers()
        {
            appDispatcher.DispatchUI(() =>
            {
                var spellToRemove = SpellList
                    .OfType<SpellViewModel>()
                    .Where(a => !a.CastByYou())
                    .ToList();
                
                foreach (var spell in spellToRemove)
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
                if (overWrite)
                {
                    if (SpellList.OfType<SpellViewModel>().FirstOrDefault(a => string.Equals(a.Id, match.Id, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(match.Target, a.Target, StringComparison.OrdinalIgnoreCase)) is SpellViewModel spell)
                    {
                        _ = SpellList.Remove(spell);
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
                string.Equals(a.Id, match.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(match.Target, a.Target, StringComparison.OrdinalIgnoreCase)) is CounterViewModel s)
                {
                    s.AddCount(match.Caster);
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
                var rollsInGroup = SpellList
                    .OfType<RollViewModel>()
                    .Where(a => string.Equals(match.Target, a.Target, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var item in rollsInGroup)
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
                    var existing = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer
                    && string.Equals(a.Id, match.Id, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(match.Target, a.Target, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        _ = SpellList.Remove(existing);
                    }
                }

                SpellList.Add(match);
            });
        }

        public void AddSavedYouSpells(List<YouSpells> youSpells)
        {
            if (youSpells == null || !youSpells.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                foreach (var item in youSpells)
                {
                    if (!spells.AllSpells.TryGetValue(item.Name, out var match))
                    {
                        continue;
                    }

                    var spellDuration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                    var savedSpellDuration = item.TotalSecondsLeft;
                    var uiSpell = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        BenefitDetriment = match.benefit_detriment,
                        SpellType = match.SpellType,
                        Id = match.name,
                        Target = EQSpells.SpaceYou,
                        Caster = item.Caster,
                        Rect = match.Rect,
                        Icon = match.SpellIcon,
                        Classes = match.Classes,
                        TotalDuration = spellDuration,
                        TotalRemainingDuration = TimeSpan.FromSeconds(savedSpellDuration)
                    };
                        
                    SpellList.Add(uiSpell);
                }
            });
        }

        public void TryRemoveUnambiguousSpellOther(string possibleSpell)
        {
            if (string.IsNullOrWhiteSpace(possibleSpell))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList
                    .OfType<SpellViewModel>()
                    .Where(spell => string.Equals(spell.Id, possibleSpell, StringComparison.OrdinalIgnoreCase) && !spell.CastOnYou())
                    .ToList();
                
                if (spells.Count == 1)
                {
                    _ = SpellList.Remove(spells.FirstOrDefault());
                }
            });
        }

        public void TryRemoveUnambiguousSpellOther(List<string> possibleSpellNames)
        {
            if (!possibleSpellNames.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList
                    .OfType<SpellViewModel>()
                    .Where(spell => possibleSpellNames.Any(name => string.Equals(spell.Id, name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                if (spells.Count == 1)
                {
                    _ = SpellList.Remove(spells.FirstOrDefault());
                }
            });
        }

        public void TryRemoveUnambiguousSpellSelf(List<string> possibleSpellNames)
        {
            if (!possibleSpellNames.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList.OfType<SpellViewModel>()
                    .Where(spell => possibleSpellNames.Any(name => spell.CastOnYou() && string.Equals(spell.Id, name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                if (spells.Count == 1)
                {
                    _ = SpellList.Remove(spells.FirstOrDefault());
                }
            });
        }
        
        public void TryRemoveByPartialSpellNamesSelf(List<string> partialSpellNames)
        {
            if (!partialSpellNames.Any())
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var spells = SpellList.OfType<SpellViewModel>()
                    .Where(spell => spell.CastOnYou() && partialSpellNames.Any(name => spell.Id.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                
                _ = SpellList.Remove(spells.FirstOrDefault());
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
                    var boats = _SpellList.OfType<BoatViewModel>().ToList();
                    foreach (var boat in boatsapi)
                    {
                        boatScheduleService.UpdateBoatInformation(boat, boats, DateTimeOffset.Now);
                    }
                });
                var timersData = pigParseApi.GetRollTimers(s.Value);
                appDispatcher.DispatchUI(() =>
                {
                    var existing = SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer && (a.Id.StartsWith(CustomTimer.Ring8) || a.Id.StartsWith(CustomTimer.ScoutTime))).ToList();
                    foreach (var item in existing)
                    {
                        _ = SpellList.Remove(item);
                    }

                    if (RollTimerIcon == null)
                    {
                        spells.AllSpells.TryGetValue("Feign Death", out RollTimerIcon);
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
                            Target = CustomTimer.CustomerTime,
                            Id = CustomTimer.Ring8 + (timer.Guess ? " (Guess)" : ""),
                            Rect = RollTimerIcon.Rect,
                            Icon = RollTimerIcon.SpellIcon,
                            TotalDuration = TimeSpan.FromHours(10),
                            TotalRemainingDuration = TimeSpan.FromSeconds(minimumTargetsForRaidMode),
                            UpdatedDateTime = DateTime.Now,
                            ProgressBarColor = Brushes.LightGreen
                        };

                        match.Id = CustomTimer.ScoutTime;
                        match.TotalDuration = TimeSpan.FromHours(10);
                        if (timer.DateTime > DateTimeOffset.Now)
                        {
                            match.TotalRemainingDuration = timer.DateTime - DateTimeOffset.Now;
                        }
                        else
                        {
                            match.TotalRemainingDuration = TimeSpan.FromHours(10);
                            match.Id = $"Scout Charisa Timer (UNKNOWN)";
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
                            Target = CustomTimer.CustomerTime,
                            Id = CustomTimer.Ring8 + (timer.Guess ? " (Guess)" : ""),
                            Rect = RollTimerIcon.Rect,
                            Icon = RollTimerIcon.SpellIcon,
                            TotalDuration = TimeSpan.FromHours(10),
                            TotalRemainingDuration = TimeSpan.FromSeconds(minimumTargetsForRaidMode),
                            UpdatedDateTime = DateTime.Now,
                            ProgressBarColor = Brushes.LightGreen
                        };

                        match.Id = CustomTimer.Ring8;
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
        
        private void QueueFullSpellListReevaluationIfNecessary()
        {
            if (IsAutomaticMode())
            {
                engine.Recategorize();
            }
            else
            {
                // Non-automatic mode: handle individually
                foreach (var spell in SpellList.OfType<SpellViewModel>())
                    engine.ApplyNonAutomaticGroupingRule(spell);
            }
        }
        
        private CancellationTokenSource timersModifiedDebounceTs;
        private void QueueSpellGroupingReevaluation(int delay)
        {
            // We need to queue the re-evaluation with a debounce cancellation because we don't want to be doing constant iteration over the whole list while it is actively being modified.
            // There is also an issue when removing whole groups that can cause the removal to remove the wrong items because the ReevaluateRelatedGroupings function actively reshapes the groupings after every pass.
            // This debounce is necessary to ensure we do not waste unnecessary time or cause undesirable side effects when doing bulk operations or when several timers fall off at roughly the same time.
            
            var evaluationItemCount = recentlyImpactedSpells.Count;
            appDispatcher.DebounceToUI(ref timersModifiedDebounceTs, delay, () =>
            {
                var safeCloneOfRecentlyImpacted = recentlyImpactedSpells.ToList();  // Clone it since we're about to wipe it clean
                recentlyImpactedSpells.Clear();
                
                engine.Recategorize(safeCloneOfRecentlyImpacted);
            },
            shouldCancel: () => evaluationItemCount != recentlyImpactedSpells.Count);
        }
        
        private bool IsAutomaticMode() => settings.PlayerSpellGroupingType == SpellGroupingType.Automatic || settings.NpcSpellGroupingType == SpellGroupingType.Automatic;
        
        private void SpellList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newSpells = (e.NewItems ?? Array.Empty<SpellViewModel>()).OfType<SpellViewModel>();
            var removedSpells = (e.OldItems ?? Array.Empty<SpellViewModel>()).OfType<SpellViewModel>();

            engine.AddSpells(newSpells);
            engine.RemoveSpells(removedSpells);

            recentlyImpactedSpells.UnionWith(newSpells.Concat(removedSpells));

            if (recentlyImpactedSpells.Any())
                QueueSpellGroupingReevaluation(groupRevaluationDebounceTime);
        }
        
        private void Base_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsCurrentlyClickThrough))
            {
                OnPropertyChanged(nameof(GenericButtonVisibility));
            }
            else if (e.PropertyName == nameof(settings.PlayerSpellGroupingType)
            || e.PropertyName == nameof(settings.NpcSpellGroupingType)
            || e.PropertyName == nameof(settings.SpellsFilter))
            {
                if (_SpellList == null)
                    return;
                
                QueueFullSpellListReevaluationIfNecessary();
            }
        }
    }
}
