using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EQTool.UI
{
    public partial class SpellWindow : BaseSaveStateWindow
    {
        private readonly System.Timers.Timer UITimer;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly TimersService timersService;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly IAppDispatcher appDispatcher;

        public SpellWindow(
            PlayerTrackerService playerTrackerService,
            TimersService timersService,
            EQToolSettings settings,
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            LoggingService loggingService) : base(settings.SpellWindowState, toolSettingsLoad, settings)
        {
            loggingService.Log(string.Empty, EventType.OpenMap, activePlayer?.Player?.Server);
            this.playerTrackerService = playerTrackerService;
            this.timersService = timersService;
            this.logEvents = logEvents;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<PersistentViewModel>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            if (this.activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(this.activePlayer.Player.YouSpells);
            }
            InitializeComponent();
            base.Init();
            this.logEvents.SpellWornOffOtherEvent += LogParser_SpellWornOtherOffEvent;
            this.logEvents.CampEvent += LogParser_CampEvent;
            this.logEvents.EnteredWorldEvent += LogParser_EnteredWorldEvent;
            this.logEvents.SpellWornOffSelfEvent += LogParser_SpellWornOffSelfEvent;
            this.logEvents.SpellCastEvent += LogParser_StartCastingEvent;
            this.logEvents.SlainEvent += LogParser_DeathEvent;
            this.logEvents.StartTimerEvent += LogParser_StartTimerEvent;
            this.logEvents.CancelTimerEvent += LogParser_CancelTimerEvent;
            this.logEvents.DeathTouchEvent += LogParser_POFDTEvent;
            this.logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
            this.logEvents.RandomRollEvent += LogParser_RandomRollEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TimerViewModel.GroupName)));
            view.LiveGroupingProperties.Add(nameof(TimerViewModel.GroupName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.Sorting), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(RollViewModel.Roll), ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription(nameof(TimerViewModel.TotalRemainingDuration), ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(TimerViewModel.TotalRemainingDuration));
        }

        private void LogParser_RandomRollEvent(object sender, RandomRollEvent e)
        {
            spellWindowViewModel.TryAddCustom(new CustomTimer
            {
                TargetName = $"Random -- {e.MaxRoll}",
                Name = e.PlayerName,
                SpellNameIcon = "Invisibility",
                SpellType = EQToolShared.Enums.SpellTypes.RandomRoll,
                Roll = e.Roll,
                DurationInSeconds = 60 * 3
            });
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            if (e.isYou)
            {
                spellWindowViewModel.TryAdd(new SpellCastEvent
                {
                    CastByYou = e.isYou,
                    Spell = e.Spell,
                    MultipleMatchesFound = false,
                    TargetName = EQSpells.SpaceYou,
                }, true);
            }
        }

        private void LogParser_POFDTEvent(object sender, DeathTouchEvent e)
        {
            spellWindowViewModel.TryAddCustom(new CustomTimer
            {
                DurationInSeconds = 45,
                Name = $"--DT-- '{e.DTReceiver}'",
                SpellNameIcon = "Disease Cloud",
                SpellType = EQToolShared.Enums.SpellTypes.BadGuyCoolDown
            });
        }

        private void LogParser_CampEvent(object sender, CampEvent e)
        {
            appDispatcher.DispatchUI(() =>
            {
                TrySaveYouSpellData();
                base.SaveState();
                spellWindowViewModel.ClearYouSpells();
            });
        }

        private void LogParser_EnteredWorldEvent(object sender, EnteredWorldEvent e)
        {
            spellWindowViewModel.ClearYouSpells();
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
        }

        private void LogParser_SpellWornOffSelfEvent(object sender, SpellWornOffSelfEvent e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }

        private void LogParser_SpellWornOtherOffEvent(object sender, SpellWornOffOtherEvent e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
        }

        private void LogParser_StartCastingEvent(object sender, SpellCastEvent e)
        {
            spellWindowViewModel.TryAdd(e, false);
        }

        private int deathcounter = 1;
        private void LogParser_DeathEvent(object sender, SlainEvent e)
        {
            spellWindowViewModel.TryRemoveTarget(e.Victim);
            if (playerTrackerService.IsPlayer(e.Victim) || !MasterNPCList.NPCs.Contains(e.Victim))
            {
                return;
            }
            var zonetimer = ZoneSpawnTimes.GetSpawnTime(e.Victim, activePlayer?.Player?.Zone);
            var add = new CustomTimer
            {
                Name = "--Dead-- " + e.Victim,
                DurationInSeconds = (int)zonetimer.TotalSeconds,
                SpellNameIcon = "Disease Cloud",
                SpellType = EQToolShared.Enums.SpellTypes.RespawnTimer
            };

            var exisitngdeathentry = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == add.Name && CustomTimer.CustomerTime == a.GroupName);
            if (exisitngdeathentry != null)
            {
                deathcounter = ++deathcounter > 999 ? 1 : deathcounter;
                add.Name += "_" + deathcounter;
            }

            spellWindowViewModel.TryAddCustom(add);
        }

        private void LogParser_CancelTimerEvent(object sender, CancelTimerEvent e)
        {
            spellWindowViewModel.TryRemoveCustom(e.Name);
        }

        private void LogParser_StartTimerEvent(object sender, StartTimerEvent e)
        {
            spellWindowViewModel.TryAddCustom(e.CustomTimer);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            if (logEvents != null)
            {
                logEvents.SpellWornOffOtherEvent -= LogParser_SpellWornOtherOffEvent;
                logEvents.CampEvent -= LogParser_CampEvent;
                logEvents.EnteredWorldEvent -= LogParser_EnteredWorldEvent;
                logEvents.SpellWornOffSelfEvent -= LogParser_SpellWornOffSelfEvent;
                logEvents.SpellCastEvent -= LogParser_StartCastingEvent;
                logEvents.SlainEvent -= LogParser_DeathEvent;
                logEvents.StartTimerEvent -= LogParser_StartTimerEvent;
                logEvents.CancelTimerEvent -= LogParser_CancelTimerEvent;
                logEvents.DeathTouchEvent -= LogParser_POFDTEvent;
                logEvents.ResistSpellEvent -= LogParser_ResistSpellEvent;
                logEvents.RandomRollEvent -= LogParser_RandomRollEvent;
            }
            if (spellWindowViewModel != null)
            {
                spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<PersistentViewModel>();
            }
            base.OnClosing(e);
        }

        private void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                //var before = activePlayer.Player.YouSpells ?? new System.Collections.Generic.List<YouSpells>();
                //activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Where(a => a.GroupName == EQSpells.SpaceYou).Select(a => new YouSpells
                //{
                //    Name = a.Name,
                //    TotalSecondsLeft = (int)a.TotalRemainingDuration.TotalSeconds,
                //}).ToList();
            }
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }

        private void RemoveSingleItem(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext;
            _ = spellWindowViewModel.SpellList.Remove(name as PersistentViewModel);
        }

        private void RemoveFromSpells(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button).DataContext as dynamic)?.Name as string;
            var items = spellWindowViewModel.SpellList.Where(a => a.GroupName == name).ToList();
            foreach (var item in items)
            {
                _ = spellWindowViewModel.SpellList.Remove(item);
            }
        }
    }
}
