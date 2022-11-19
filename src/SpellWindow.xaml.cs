using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
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

        public SpellWindow(EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, LogParser logParser, SpellLogParse spellLogParse, LogDeathParse logDeathParse)
        {
            this.logDeathParse = logDeathParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
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

            view.GroupDescriptions.Add(new PropertyGroupDescription("SortingOrder"));
            view.LiveGroupingProperties.Add("SortingOrder");
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription("SortingOrder", ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add("SortingOrder");

        }
        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = spellLogParse.MatchSpell(e.Line);
            if (matched?.Spell != null)
            {
                spellWindowViewModel.TryAdd(matched);
            }

            var targettoremove = logDeathParse.GetDeadTarget(e.Line);
            spellWindowViewModel.TryRemoveTarget(targettoremove);
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
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            var sortname = ((dynamic)((Button)sender).DataContext)?.Name;
            foreach (var item in spellWindowViewModel.SpellList.Where(a => a.SortingOrder == sortname))
            {
                item.Collapse = !item.Collapse;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("sdfsdf");
        }
    }
}
