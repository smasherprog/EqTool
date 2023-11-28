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
                DateTime = new DateTime(2023, 11, 25),
                Message = "If kael faction pulls are happening, a timer will be added to let you know when the next will be regardless of whether you are in the zone"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 25),
                Message = "Fixed various map issues"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 25),
                Message = "Added ability to remove individual items from the Triggers window"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 25),
                Message = "Change Auto Update to only occur if you have been idle for 2 or more minutes!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Adjusted Trashcan timer alignment again. It was off when timer was over 1 hour."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Various bug fixes. Your settings probably were cleared in the last update, sorry about that!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Library updates, various underlying code refactorings, removed dead code to support light theme (Yuck)."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Library updates, various underlying code refactorings, removed dead code to support light theme (Yuck)."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Fixed map zoom and drag bug where tracking radius size changed incorrectly."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Aligned delete buttons in triggers window for easier mass deletes!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 26),
                Message = "Added Minwidth to the mob info loot window as sometimes it would be too small."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 27),
                Message = "Fixed issue where empty spells were showing up in the list."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 11, 27),
                Message = "Fixed Death messages for faction mobs."
            });
        }
    }
}
