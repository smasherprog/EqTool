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
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* Added test buttons next to the new overlay options. You must have the option enabled when you test otherwise, nothing will happen."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* Added audio alerts for Enrage, Levitate fading and Invis Fading!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* Added overlay option for Levitate fading!{Environment.NewLine}" +
                $"* Added overlay option for Invis fading!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* PQ Instance Zones added to the map. Let me know if I missed any!{Environment.NewLine}" +
                $"* Always on top options added back.{Environment.NewLine}" +
                $"* Fixed bug reported where Druid epic was not showing up correctly. This also was effecting other epics in some scenarios so those are fixed as well."
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 5),
                Message = $"* Fixed CH time animation on the overlay. It should be 10 seconds from right to left."
            });

            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 6),
                Message = $"* Try out the new overlay and audio triggers. Try out the CH Chain overlay as well. {Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                          $"* FTE audio and visual triggers added. Visual Trigger will show the guild of tagger!{Environment.NewLine}" +
                          $"* Remove ghost players from map.{Environment.NewLine}" +
                          $"* Charm Break Audio and visual triggers added.{Environment.NewLine}" +
                          $"* Updated save settings code to be smarter!{Environment.NewLine}" +
                          $"* Changed Audio triggers to use Crappy Built in speech to text -- this is easier than generating MP3 for everything.{Environment.NewLine}" +
                          $"* Added code to try and improve the map locations by simplifying code.{Environment.NewLine}" +
                          $"* Changed code to check for updates. Now it will only update if there are less than 2 TOTAL items in the triggers window to make less aggressive.",
                Image = "pack://application:,,,/update1.png",
                ImageVisibility = Visibility.Visible
            });
        }
    }
}
