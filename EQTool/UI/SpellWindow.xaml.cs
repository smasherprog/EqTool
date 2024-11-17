using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;
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

        public SpellWindow(
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
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<PersistentViewModel>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
            InitializeComponent();
            base.Init();
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

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            if (spellWindowViewModel != null)
            {
                spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<PersistentViewModel>();
            }
            base.OnClosing(e);
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
