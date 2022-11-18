using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class DPSMeter : Window
    {
        private readonly Timer UITimer;

        private readonly LogParser logParser;
        private readonly DPSWindowViewModel dPSWindowViewModel;
        private readonly DPSLogParse dPSLogParse;
        private SortAdorner listViewSortAdorner = null;
        private GridViewColumnHeader listViewSortCol = null;

        public DPSMeter(DPSLogParse dPSLogParse, LogParser logParser, DPSWindowViewModel dPSWindowViewModel)
        {
            this.dPSLogParse = dPSLogParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            this.dPSWindowViewModel = dPSWindowViewModel;
            DataContext = dPSWindowViewModel;
            InitializeComponent();

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            DpsList.ItemsSource = dPSWindowViewModel.EntityList;
            dPSWindowViewModel.EntityList.CollectionChanged += items_CollectionChanged;
            _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(delegate (object sender, ExecutedRoutedEventArgs args) { Close(); })));
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(dPSWindowViewModel.EntityList);
            view.GroupDescriptions.Add(new PropertyGroupDescription("TargetName"));
            view.LiveGroupingProperties.Add("TargetName");
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription("TargetName", ListSortDirection.Ascending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add("TargetName");
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = dPSLogParse.Match(e.Line);
            dPSWindowViewModel.TryAdd(matched);
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
            UITimer.Dispose();
            dPSWindowViewModel.EntityList.CollectionChanged -= items_CollectionChanged;
            logParser.LineReadEvent += LogParser_LineReadEvent;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            dPSWindowViewModel.UpdateDPS();
        }

        private void nameheaderclick(object sender, EventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            var sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                DpsList.Items.SortDescriptions.Clear();
            }

            var newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            DpsList.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        public class SortAdorner : Adorner
        {
            private static readonly Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static readonly Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            {
                Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                {
                    return;
                }

                var transform = new TranslateTransform
                    (
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2
                    );
                drawingContext.PushTransform(transform);

                var geometry = ascGeometry;
                if (Direction == ListSortDirection.Descending)
                {
                    geometry = descGeometry;
                }

                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        }
    }
}
