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
                DateTime = new DateTime(2023, 12, 7),
                Message = "Fixed PQ bug that prevent program from loading. Next time post an issue on Github and it will be fixed in minutes!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 7),
                Message = "Fixed Hammer of the Dragonborn clicky Primal Essence will now show up when used!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 9),
                Message = "poh zone timers updated."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 9),
                Message = "a protector of growth spawn timers added to pog."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 9),
                Message = "Adjusted crystal caverns map opacity levels."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 11),
                Message = "Updated code so that PQ zone timers can be added by those without coding ability. Check out github!"
            }); 
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2023, 12, 13),
                Message = "Updated various zone timers!"
            });
        }
    }
}
