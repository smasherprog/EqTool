using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        private long? LastLogReadOffset;

        public SpellWindow(EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, LogParser logParser, EQToolSettingsLoad toolSettingsLoad, ActivePlayer activePlayer)
        {
            this.settings = settings;
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.logParser.SpellWornOtherOffEvent += LogParser_SpellWornOtherOffEvent;
            this.logParser.SpellWornOffSelfEvent += LogParser_SpellWornOffSelfEvent;
            this.logParser.StartCastingEvent += LogParser_StartCastingEvent;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            this.logParser.StartTimerEvent += LogParser_StartTimerEvent;
            this.logParser.CancelTimerEvent += LogParser_CancelTimerEvent;
            this.logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent; 
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel; 
            if (this.activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(this.activePlayer.Player.YouSpells);
            }
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.SpellWindowState, this);
            Topmost = Properties.Settings.Default.GlobalTriggerWindowAlwaysOnTop;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UISpell.TargetName)));
            view.LiveGroupingProperties.Add(nameof(UISpell.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.TargetName), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.SecondsLeftOnSpell), ListSortDirection.Descending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(UISpell.SecondsLeftOnSpell));
            this.toolSettingsLoad = toolSettingsLoad;
            SizeChanged += DPSMeter_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += DPSMeter_LocationChanged;
            settings.SpellWindowState.Closed = false;
      
            SaveState();
        }
         
        private void LogParser_SpellWornOffSelfEvent(object sender, LogParser.SpellWornOffSelfEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }

        private void LogParser_SpellWornOtherOffEvent(object sender, LogParser.SpellWornOffOtherEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
        }
         
        private void LogParser_StartCastingEvent(object sender, LogParser.StartCastingEventArgs e)
        {
            spellWindowViewModel.TryAdd(e.Spell);
        }

        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
        {
            spellWindowViewModel.TryRemoveTarget(e.Name);
        }

        private void LogParser_CancelTimerEvent(object sender, LogParser.CancelTimerEventArgs e)
        {
            spellWindowViewModel.TryRemoveCustom(e.Name);
        }

        private void LogParser_StartTimerEvent(object sender, LogParser.StartTimerEventArgs e)
        {
            spellWindowViewModel.TryAddCustom(e.CustomerTimer);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            logParser.SpellWornOtherOffEvent -= LogParser_SpellWornOtherOffEvent;
            logParser.SpellWornOffSelfEvent -= LogParser_SpellWornOffSelfEvent;
            logParser.StartCastingEvent -= LogParser_StartCastingEvent;
            logParser.DeadEvent -= LogParser_DeadEvent;
            logParser.StartTimerEvent -= LogParser_StartTimerEvent;
            logParser.CancelTimerEvent -= LogParser_CancelTimerEvent;
            logParser.PlayerChangeEvent -= LogParser_PlayerChangeEvent;
            SaveState();
            base.OnClosing(e);
        }

        private void TrySaveYouSpellData()
        {
            long? oldLastLogReadOffset = this.LastLogReadOffset;
            var logerLastLogReadOffset = this.logParser.LastLogReadOffset;
            if (this.activePlayer.Player != null && oldLastLogReadOffset != logerLastLogReadOffset)
            {
                Debug.WriteLine("Saving You Spell Data");
                this.LastLogReadOffset = logerLastLogReadOffset;
                this.activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Select(a => new YouSpells
                {
                    Name = a.SpellName,
                    TotalSecondsLeft = (int)a.SecondsLeftOnSpell.TotalSeconds,
                }).ToList();
            }
        }

        private void SaveState()
        {
            TrySaveYouSpellData();
            WindowExtensions.SaveWindowState(settings.SpellWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            settings.SpellWindowState.Closed = true;
            Close();
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_LocationChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveState();
        }

        private void LogParser_PlayerChangeEvent(object sender, LogParser.PlayerChangeEventArgs e)
        { 
            spellWindowViewModel.ClearAllSpells();
            this.LastLogReadOffset = this.logParser.LastLogReadOffset;
            if (this.activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(this.activePlayer.Player.YouSpells);
            }
        }

        private void PollUI(object sender, EventArgs e)
        {
            TrySaveYouSpellData();
            toolSettingsLoad.Save(settings);
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
