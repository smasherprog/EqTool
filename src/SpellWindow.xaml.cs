using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using EQToolShared.Map;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EQTool
{
    public partial class SpellWindow : BaseSaveStateWindow
    {
        private readonly System.Timers.Timer UITimer;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EventsList eventsList;
        private readonly ActivePlayer activePlayer;
        private readonly TimersService timersService;
        private readonly PlayerTrackerService playerTrackerService;

        public SpellWindow(
            PlayerTrackerService playerTrackerService,
            TimersService timersService,
            EQToolSettings settings,
            SpellWindowViewModel spellWindowViewModel,
            EventsList eventsList,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            LoggingService loggingService) : base(settings.SpellWindowState, toolSettingsLoad, settings)
        {
            loggingService.Log(string.Empty, EventType.OpenMap, activePlayer?.Player?.Server);
            this.playerTrackerService = playerTrackerService;
            this.timersService = timersService;
            this.eventsList = eventsList;
            this.activePlayer = activePlayer;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            if (this.activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(this.activePlayer.Player.YouSpells);
            }
            InitializeComponent();
            base.Init();
            this.eventsList.SpellWornOtherOffEvent += LogParser_SpellWornOtherOffEvent;
            this.eventsList.CampEvent += LogParser_CampEvent;
            this.eventsList.EnteredWorldEvent += LogParser_EnteredWorldEvent;
            this.eventsList.SpellWornOffSelfEvent += LogParser_SpellWornOffSelfEvent;
            this.eventsList.StartCastingEvent += LogParser_StartCastingEvent;
            this.eventsList.DeadEvent += LogParser_DeadEvent;
            this.eventsList.StartTimerEvent += LogParser_StartTimerEvent;
            this.eventsList.CancelTimerEvent += LogParser_CancelTimerEvent;
            this.eventsList.POFDTEvent += LogParser_POFDTEvent;
            this.eventsList.ResistSpellEvent += LogParser_ResistSpellEvent;
            this.eventsList.RandomRollEvent += LogParser_RandomRollEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UISpell.TargetName)));
            view.LiveGroupingProperties.Add(nameof(UISpell.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.Sorting), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.Roll), ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.SecondsLeftOnSpell), ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(UISpell.SecondsLeftOnSpell));
        }

        private void LogParser_RandomRollEvent(object sender, EventsList.RandomRollEventArgs e)
        {
            this.spellWindowViewModel.TryAddCustom(new CustomTimer
            {
                TargetName = $"Random -- {e.RandomRollData.MaxRoll}",
                Name = e.RandomRollData.PlayerName,
                SpellNameIcon = "Invisibility",
                SpellType = EQToolShared.Enums.SpellTypes.RandomRoll,
                Roll = e.RandomRollData.Roll,
                DurationInSeconds = 60 * 3
            });
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellParser.ResistSpellData e)
        {
            if (e.isYou)
            {
                spellWindowViewModel.TryAdd(new SpellParsingMatch
                {
                    IsYou = e.isYou,
                    Spell = e.Spell,
                    MultipleMatchesFound = false,
                    TargetName = EQSpells.SpaceYou,
                }, true);
            }
        }

        private void LogParser_POFDTEvent(object sender, POFDTParser.POF_DT_Event e)
        {
            this.spellWindowViewModel.TryAddCustom(new CustomTimer
            {
                DurationInSeconds = 45,
                Name = $"--DT-- '{e.DTReceiver}'",
                SpellNameIcon = "Disease Cloud",
                SpellType = EQToolShared.Enums.SpellTypes.BadGuyCoolDown
            });
        }

        private void LogParser_CampEvent(object sender, EventsList.CampEventArgs e)
        {
            TrySaveYouSpellData();
            base.SaveState();
            spellWindowViewModel.ClearYouSpells();
        }

        private void LogParser_EnteredWorldEvent(object sender, EventsList.EnteredWorldArgs e)
        {
            spellWindowViewModel.ClearYouSpells();
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
        }

        private void LogParser_SpellWornOffSelfEvent(object sender, EventsList.SpellWornOffSelfEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }

        private void LogParser_SpellWornOtherOffEvent(object sender, EventsList.SpellWornOffOtherEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
        }

        private void LogParser_StartCastingEvent(object sender, EventsList.SpellEventArgs e)
        {
            spellWindowViewModel.TryAdd(e.Spell, false);
        }

        private int deathcounter = 1;
        private void LogParser_DeadEvent(object sender, EventsList.DeadEventArgs e)
        {
            spellWindowViewModel.TryRemoveTarget(e.Name);
            if (playerTrackerService.IsPlayer(e.Name) || !MasterNPCList.NPCs.Contains(e.Name))
            {
                return;
            }
            var zonetimer = ZoneSpawnTimes.GetSpawnTime(e.Name, activePlayer?.Player?.Zone);
            var add = new CustomTimer
            {
                Name = "--Dead-- " + e.Name,
                DurationInSeconds = (int)zonetimer.TotalSeconds,
                SpellNameIcon = "Disease Cloud",
                SpellType = EQToolShared.Enums.SpellTypes.RespawnTimer
            };

            var exisitngdeathentry = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellName == add.Name && CustomTimer.CustomerTime == a.TargetName);
            if (exisitngdeathentry != null)
            {
                deathcounter = ++deathcounter > 999 ? 1 : deathcounter;
                add.Name += "_" + deathcounter;
            }

            spellWindowViewModel.TryAddCustom(add);
        }

        private void LogParser_CancelTimerEvent(object sender, EventsList.CancelTimerEventArgs e)
        {
            spellWindowViewModel.TryRemoveCustom(e.Name);
        }

        private void LogParser_StartTimerEvent(object sender, EventsList.StartTimerEventArgs e)
        {
            spellWindowViewModel.TryAddCustom(e.CustomTimer);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            if (eventsList != null)
            {
                eventsList.CampEvent -= LogParser_CampEvent;
                eventsList.SpellWornOtherOffEvent -= LogParser_SpellWornOtherOffEvent;
                eventsList.EnteredWorldEvent -= LogParser_EnteredWorldEvent;
                eventsList.SpellWornOffSelfEvent -= LogParser_SpellWornOffSelfEvent;
                eventsList.StartCastingEvent -= LogParser_StartCastingEvent;
                eventsList.DeadEvent -= LogParser_DeadEvent;
                eventsList.StartTimerEvent -= LogParser_StartTimerEvent;
                eventsList.CancelTimerEvent -= LogParser_CancelTimerEvent;
                eventsList.POFDTEvent -= LogParser_POFDTEvent;
                eventsList.ResistSpellEvent -= LogParser_ResistSpellEvent;
                eventsList.RandomRollEvent -= LogParser_RandomRollEvent;
            }
            if (spellWindowViewModel != null)
            {
                spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            }
            base.OnClosing(e);
        }

        private void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                var before = activePlayer.Player.YouSpells ?? new System.Collections.Generic.List<YouSpells>();
                activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Where(a => a.TargetName == EQSpells.SpaceYou).Select(a => new YouSpells
                {
                    Name = a.SpellName,
                    TotalSecondsLeft = (int)a.SecondsLeftOnSpell.TotalSeconds,
                }).ToList();
            }
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }

        private void RemoveSingleItem(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext;
            spellWindowViewModel.SpellList.Remove(name as UISpell);
        }

        private void RemoveFromSpells(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button).DataContext as dynamic)?.Name as string;
            var items = spellWindowViewModel.SpellList.Where(a => a.TargetName == name).ToList();
            foreach (var item in items)
            {
                _ = spellWindowViewModel.SpellList.Remove(item);
            }
        }
    }
}
