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
        public Visibility ImageVisibility { get; set; } = Visibility.Collapsed;

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
                DateTime = new DateTime(2024, 1, 4),
                Message = $"*Updating CH overlay to pick up guild and some UI improvements.{Environment.NewLine}" +
                $"*CH overlay picks up OOC, Shout and Guild. If you want to change the TAG used to pickup on the chain goto settings and add a tag there!{Environment.NewLine}" +
                $"*CH Chain overlay is still a work in progress, but it actually works now. Remaining work is to improve the look, functionally, it works!{Environment.NewLine}"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 4),
                Message = $"*Removed window always on top checkbox. All windows will be on top now.{Environment.NewLine}" +
                $"*The overlay window is transparent except for an EVIL RED DOT. If you hover over the dot, it will show the bounds of the overlay which can be used to position OVER your EQ window.{Environment.NewLine}" +
                $"*The Red dot window will be where Enraged, FTE, Invis drop, Charm break, messages go once implemented. Also, this is where the CH CHain overlay will go.{Environment.NewLine}" +
                $"*CH Chain overlay work is NOT DONE!!!{Environment.NewLine}" +
                $"*Work on the CH Chain overlay continues, you can see how it works THUS far. Colors and behaviors are still being worked on and it currently looks like shit!.{Environment.NewLine}"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* Fixed CH time animcation on the overlay. It should be 10 seconds from right to left."
            });
        }
    }
}
