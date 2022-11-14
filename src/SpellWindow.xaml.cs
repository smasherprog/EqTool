using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        private readonly Timer ParseTimer;
        private readonly Timer UITimer;

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly SpellLogParse spellLogParse;
        private readonly IAppDispatcher appDispatcher;
        private readonly LogDeathParse logDeathParse;

        public SpellWindow(EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer, SpellLogParse spellLogParse, IAppDispatcher appDispatcher, LogDeathParse logDeathParse)
        {
            this.logDeathParse = logDeathParse;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.spellLogParse = spellLogParse;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            _ = this.activePlayer.Update();
            this.spellWindowViewModel = spellWindowViewModel;
            DataContext = spellWindowViewModel;
            Topmost = settings.TriggerWindowTopMost;
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(delegate (object sender, ExecutedRoutedEventArgs args) { Close(); })));

            PollUpdates(null, null);
            ParseTimer = new System.Timers.Timer(500);
            ParseTimer.Elapsed += PollUpdates;
            ParseTimer.Enabled = true;

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

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            ParseTimer.Stop();
            UITimer.Dispose();
            ParseTimer.Dispose();
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
        }

        private void PollUpdates(object sender, EventArgs e)
        {
            var playerchanged = activePlayer.Update();
            var lastreadoffset = spellWindowViewModel.LastReadOffset;
            if (playerchanged)
            {
                appDispatcher.DispatchUI(() => { spellWindowViewModel.LastReadOffset = null; });
                lastreadoffset = null;
            }
            var filepath = activePlayer.LogFileName;
            if (string.IsNullOrWhiteSpace(filepath))
            {
                Debug.WriteLine($"No playerfile found!");
                return;
            }

            try
            {
                var fileinfo = new FileInfo(filepath);
                if (!lastreadoffset.HasValue || lastreadoffset > fileinfo.Length)
                {
                    Debug.WriteLine($"Player Switched or new Player detected");
                    lastreadoffset = fileinfo.Length;
                    appDispatcher.DispatchUI(() => { spellWindowViewModel.LastReadOffset = lastreadoffset; });
                }
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    _ = stream.Seek(lastreadoffset.Value, SeekOrigin.Begin);
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lastreadoffset = stream.Position;
                        appDispatcher.DispatchUI(() => { spellWindowViewModel.LastReadOffset = lastreadoffset; });
                        if (line.Length > 27)
                        {
                            var matched = spellLogParse.MatchSpell(line);
                            if (matched?.Spell != null)
                            {
                                spellWindowViewModel.TryAdd(matched);
                            }

                            var targettoremove = logDeathParse.GetDeadTarget(line);
                            spellWindowViewModel.TryRemoveTarget(targettoremove);
                        }
                    }
                }
            }
            catch { }
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            var sortname = ((dynamic)((Button)sender).DataContext)?.Name;
            foreach (var item in spellWindowViewModel.SpellList.Where(a => a.SortingOrder == sortname))
            {
                item.Collapse = !item.Collapse;
            }
        }
    }
}
