using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EQTool
{
    public partial class SpellWindow : Window
    {
        private readonly Timer UITimer;

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly LogParser logParser;
        private readonly SpellLogParse spellLogParse;
        private readonly LogDeathParse logDeathParse;
        private readonly LogCustomTimer logCustomTimer;

        public SpellWindow(EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, LogParser logParser, SpellLogParse spellLogParse, LogDeathParse logDeathParse, LogCustomTimer logCustomTimer)
        {
            this.logCustomTimer = logCustomTimer;
            this.logDeathParse = logDeathParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            this.logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
            this.spellLogParse = spellLogParse;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            this.spellWindowViewModel = spellWindowViewModel;
            DataContext = spellWindowViewModel;
            Topmost = settings.TriggerWindowTopMost;
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(delegate (object sender, ExecutedRoutedEventArgs args) { Close(); })));

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);

            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UISpell.TargetName)));
            view.LiveGroupingProperties.Add(nameof(UISpell.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.TargetName), ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(UISpell.TargetName));

        }

        private void LogParser_PlayerChangeEvent(object sender, LogParser.PlayerChangeEventArgs e)
        {
            spellWindowViewModel.ClearAllSpells();
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = spellLogParse.MatchSpell(e.Line);
            spellWindowViewModel.TryAdd(matched);

            var targettoremove = logDeathParse.GetDeadTarget(e.Line);
            spellWindowViewModel.TryRemoveTarget(targettoremove);

            var customtimer = logCustomTimer.GetStartTimer(e.Line);
            spellWindowViewModel.TryAddCustom(customtimer);
            var canceltimer = logCustomTimer.GetCancelTimer(e.Line);
            spellWindowViewModel.TryRemoveCustom(canceltimer);
        }


        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            logParser.LineReadEvent += LogParser_LineReadEvent;
            logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }
    }
}
