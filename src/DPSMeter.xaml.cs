using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class DPSMeter : Window
    {

        private readonly Timer ParseTimer;
        private readonly Timer UITimer;

        private readonly ActivePlayer activePlayer;
        private readonly DPSWindowViewModel dPSWindowViewModel;
        private readonly IAppDispatcher appDispatcher;
        private const int Milliseconds_Delta = 100;
        private readonly DPSLogParse dPSLogParse;

        public DPSMeter(DPSLogParse dPSLogParse, ActivePlayer activePlayer, DPSWindowViewModel dPSWindowViewModel, IAppDispatcher appDispatcher)
        {
            this.dPSLogParse = dPSLogParse;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.dPSWindowViewModel = dPSWindowViewModel;
            DataContext = dPSWindowViewModel;
            InitializeComponent();
            ParseTimer = new System.Timers.Timer(Milliseconds_Delta);
            ParseTimer.Elapsed += PollUpdates;
            ParseTimer.Enabled = true;

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            DpsList.ItemsSource = dPSWindowViewModel.EntityList;
            dPSWindowViewModel.EntityList.CollectionChanged += items_CollectionChanged;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(dPSWindowViewModel.EntityList);
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("DPS", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Total Damage", ListSortDirection.Ascending));
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var view = DpsList.View as GridView;
            AutoResizeGridViewColumns(view);
        }

        private static void AutoResizeGridViewColumns(GridView view)
        {
            if (view == null || view.Columns.Count < 1)
            {
                return;
            }
            // Simulates column auto sizing
            foreach (var column in view.Columns)
            {
                // Forcing change
                if (double.IsNaN(column.Width))
                {
                    column.Width = 1;
                }

                column.Width = double.NaN;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            ParseTimer.Stop();
            UITimer.Dispose();
            ParseTimer.Dispose();
            dPSWindowViewModel.EntityList.CollectionChanged -= items_CollectionChanged;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            dPSWindowViewModel.UpdateDPS();
        }

        private void PollUpdates(object sender, EventArgs e)
        {
            var playerchanged = activePlayer.Update();
            var lastreadoffset = dPSWindowViewModel.LastReadOffset;
            if (playerchanged)
            {
                appDispatcher.DispatchUI(() => { dPSWindowViewModel.LastReadOffset = null; });
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
                    appDispatcher.DispatchUI(() => { dPSWindowViewModel.LastReadOffset = lastreadoffset; });
                }
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    _ = stream.Seek(lastreadoffset.Value, SeekOrigin.Begin);
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lastreadoffset = stream.Position;
                        appDispatcher.DispatchUI(() => { dPSWindowViewModel.LastReadOffset = lastreadoffset; });
                        if (line.Length > 27)
                        {
                            var matched = dPSLogParse.Match(line);
                            dPSWindowViewModel.TryAdd(matched);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
