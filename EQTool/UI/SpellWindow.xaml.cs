using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EQTool.UI
{
    public partial class SpellWindow : BaseSaveStateWindow
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly SettingsWindowViewModel settingsWindowViewModel;
        private readonly SlainHandler slainHandler;

        public SpellWindow(
            TimersService timersService,
            EQToolSettings settings,
            SpellWindowViewModel spellWindowViewModel,
            SettingsWindowViewModel settingsWindowViewModel,
            LogEvents logEvents,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            SlainHandler slainHandler,
            LoggingService loggingService) : base(appDispatcher, spellWindowViewModel, settings.SpellWindowState, toolSettingsLoad, settings)
        {
            this.slainHandler = slainHandler;
            this.settingsWindowViewModel = settingsWindowViewModel;
            loggingService.Log(string.Empty, EventType.OpenMap, activePlayer?.Player?.Server);
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            InitializeComponent();
            base.Init();
        }

        private void RemoveSingleItem(object sender, RoutedEventArgs e)
        {
            var vm = (sender as Button).DataContext;
            appDispatcher.DispatchUI(() =>
            {
                _ = spellWindowViewModel.SpellList.Remove(vm as PersistentViewModel);
            });
        }

        private void RemoveFromSpells(object sender, RoutedEventArgs e)
        {
            var group = ((sender as Button).DataContext as dynamic)?.Name as string;
            appDispatcher.DispatchUI(() =>
            {
                var items = spellWindowViewModel.SpellList.Where(a => a.DisplayGroup == group).ToList();
                foreach (var item in items)
                {
                    _ = spellWindowViewModel.SpellList.Remove(item);
                }
            });
        }

        private void ClearSpellsNotCastOnYou(object sender, RoutedEventArgs e)
        {
            spellWindowViewModel.ClearSpellsNotCastOnYou();
        }
        
        private void ClearSpellsCastByOthers(object sender, RoutedEventArgs e)
        {
            spellWindowViewModel.ClearSpellsCastByOthers();
        }

        private void RaidModleToggle(object sender, RoutedEventArgs e)
        {
            settingsWindowViewModel.RaidModeDetection = !settingsWindowViewModel.RaidModeDetection;
        }

        private void AddDeathTimer(object sender, RoutedEventArgs e)
        {
            slainHandler.DoEvent(new ConfirmedDeathEvent
            {
                Killer = EQSpells.SpaceYou,
                Line = "Triggered from spells UI",
                LineCounter = -1,
                TimeStamp = DateTime.Now,
                Victim = "Zone Death Timer"
            }, true);
        }
    }
}
