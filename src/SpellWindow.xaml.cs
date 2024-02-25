using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using EQToolShared.Map;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace EQTool
{
    public partial class SpellWindow : Window
    {
        private readonly System.Timers.Timer UITimer;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly LogParser logParser;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly ActivePlayer activePlayer;
        private readonly TimersService timersService;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };

        public SpellWindow(
            PlayerTrackerService playerTrackerService,
            TimersService timersService,
            EQToolSettings settings,
            SpellWindowViewModel spellWindowViewModel,
            LogParser logParser,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            LoggingService loggingService)
        {
            loggingService.Log(string.Empty, EventType.OpenMap, activePlayer?.Player?.Server);
            this.playerTrackerService = playerTrackerService;
            this.timersService = timersService;
            this.settings = settings;
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.logParser.SpellWornOtherOffEvent += LogParser_SpellWornOtherOffEvent;
            this.logParser.CampEvent += LogParser_CampEvent;
            this.logParser.EnteredWorldEvent += LogParser_EnteredWorldEvent;
            this.logParser.SpellWornOffSelfEvent += LogParser_SpellWornOffSelfEvent;
            this.logParser.StartCastingEvent += LogParser_StartCastingEvent;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            this.logParser.StartTimerEvent += LogParser_StartTimerEvent;
            this.logParser.CancelTimerEvent += LogParser_CancelTimerEvent;
            this.logParser.POFDTEvent += LogParser_POFDTEvent;
            this.logParser.ResistSpellEvent += LogParser_ResistSpellEvent;
            this.logParser.RandomRollEvent += LogParser_RandomRollEvent;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            if (this.activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(this.activePlayer.Player.YouSpells);
            }
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.SpellWindowState, this);
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UISpell.TargetName)));
            view.LiveGroupingProperties.Add(nameof(UISpell.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.Sorting), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.SecondsLeftOnSpell), ListSortDirection.Descending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(UISpell.SecondsLeftOnSpell));
            this.toolSettingsLoad = toolSettingsLoad;
            timer.Tick += timer_Tick;
            SizeChanged += DPSMeter_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += DPSMeter_LocationChanged;
            settings.SpellWindowState.Closed = false;
        }

        private void LogParser_RandomRollEvent(object sender, LogParser.RandomRollEventArgs e)
        {
            var name = $"{e.RandomRollData.PlayerName} Rolled: ";
            if (name.Length < 35)
            {
                name.PadRight(35, ' ');
            }

            //this.spellWindowViewModel.TryAddCustom(new CustomTimer
            //{
            //    TargetName = $"Random -- {e.RandomRollData.MaxRoll}",
            //    Name = name + $" '{e.RandomRollData.Roll}'",
            //    SpellNameIcon = "Invisibility",
            //    SpellType = EQToolShared.Enums.SpellTypes.RandomRoll
            //});
        }

        private void LogParser_ResistSpellEvent(object sender, SpellParsingMatch e)
        {
            spellWindowViewModel.TryAdd(e, true);
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

        private void LogParser_CampEvent(object sender, LogParser.CampEventArgs e)
        {
            TrySaveYouSpellData();
            toolSettingsLoad.Save(settings);
            spellWindowViewModel.ClearYouSpells();
        }

        private void LogParser_EnteredWorldEvent(object sender, LogParser.EnteredWorldArgs e)
        {
            spellWindowViewModel.ClearYouSpells();
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
        }

        private void LogParser_SpellWornOffSelfEvent(object sender, LogParser.SpellWornOffSelfEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }

        private void LogParser_SpellWornOtherOffEvent(object sender, LogParser.SpellWornOffOtherEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
        }

        private void LogParser_StartCastingEvent(object sender, LogParser.SpellEventArgs e)
        {
            spellWindowViewModel.TryAdd(e.Spell, false);
        }

        private int deathcounter = 1;
        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
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

        private void LogParser_CancelTimerEvent(object sender, LogParser.CancelTimerEventArgs e)
        {
            spellWindowViewModel.TryRemoveCustom(e.Name);
        }

        private void LogParser_StartTimerEvent(object sender, LogParser.StartTimerEventArgs e)
        {
            spellWindowViewModel.TryAddCustom(e.CustomTimer);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.IsEnabled = false;
            SaveState();
        }

        private void DebounceSave()
        {
            timer.IsEnabled = true;
            timer.Stop();
            timer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            if (logParser != null)
            {
                logParser.SpellWornOtherOffEvent -= LogParser_SpellWornOtherOffEvent;
                logParser.CampEvent -= LogParser_CampEvent;
                logParser.EnteredWorldEvent -= LogParser_EnteredWorldEvent;
                logParser.SpellWornOffSelfEvent -= LogParser_SpellWornOffSelfEvent;
                logParser.StartCastingEvent -= LogParser_StartCastingEvent;
                logParser.DeadEvent -= LogParser_DeadEvent;
                logParser.StartTimerEvent -= LogParser_StartTimerEvent;
                logParser.CancelTimerEvent -= LogParser_CancelTimerEvent;
                logParser.POFDTEvent -= LogParser_POFDTEvent;
                logParser.ResistSpellEvent -= LogParser_ResistSpellEvent;
                logParser.RandomRollEvent -= LogParser_RandomRollEvent;
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

        private void SaveState()
        {
            if (settings != null)
            {
                Debug.WriteLine("Saving Triggers window State");
                TrySaveYouSpellData();
                WindowExtensions.SaveWindowState(settings.SpellWindowState, this);
                toolSettingsLoad.Save(settings);
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            settings.SpellWindowState.Closed = true;
            SaveState();
            Close();
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            DebounceSave();
        }

        private void DPSMeter_LocationChanged(object sender, EventArgs e)
        {
            DebounceSave();
        }

        private void DPSMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DebounceSave();
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
        }

        private void opendps(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenDPSWindow();
        }

        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWindow();
        }
        private void openmap(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMapWindow();
        }
        private void openmobinfo(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMobInfoWindow();
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
