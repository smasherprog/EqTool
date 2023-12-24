using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace EQTool
{
    public class UpdateMessageData : INotifyPropertyChanged
    {
        public string Date { get { return DateTime.ToShortDateString(); } }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public Visibility ImageVisibility { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public partial class UpdateMessagesWindow : Window
    {
        public ObservableCollection<UpdateMessageData> UpdateMessages { get; set; } = new ObservableCollection<UpdateMessageData>();
        public UpdateMessagesWindow()
        {
            this.Topmost = true;
            this.DataContext = this;
            InitializeComponent();
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Messages.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UpdateMessageData.Date)));
            view.LiveGroupingProperties.Add(nameof(UpdateMessageData.Message));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UpdateMessageData.DateTime), ListSortDirection.Descending));
            view.IsLiveSorting = true;

            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 24),
                Message = $"Updated colors of the triggers window to try and add a little variety! {Environment.NewLine}" +
                $"Added Lord Yelinak's 'Mind Cloud' and 'Ice Breath' effects so they show the cooldown time in triggers window {Environment.NewLine}" +
                $"Fixed Wakening land timers for Geos, Goos, and suits. Note: roamers are on a slightly different spawn time, so those will always be wrong! {Environment.NewLine}" +
                $"Added death touch events in plane of fear for Dread and Fright so they show the cooldown time in triggers window {Environment.NewLine}",
                Image = "pack://application:,,,/update1.png",
                ImageVisibility = Visibility.Visible
            });
        }
    }
}
