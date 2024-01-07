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
                DateTime = new DateTime(2024, 1, 6),
                Message = $"* Try out the new overlay and audio triggers. Try out the CH Chain overlay as well. {Environment.NewLine}" +
                          $"* FTE audio and visual triggers added. Visual Trigger will show the guild of tagger!{Environment.NewLine}" +
                          $"* Remove ghost players from map.{Environment.NewLine}" +
                          $"* Charm Break Audio and visual triggers added.{Environment.NewLine}" +
                          $"* Updated save settings code to be smarter!{Environment.NewLine}" +
                          $"* Changed Audio triggers to use Crappy Built in speech to text -- this is easier than generating MP3 for everything.{Environment.NewLine}" +
                          $"* Added code to try and improve the map locations by simplifying code.{Environment.NewLine}" +
                          $"* Changed code to check for updates. Now it will only update if there are less than 2 TOTAL items in the triggers window to make less aggressive.",
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 7),
                Message = $"* Added a CH test button so you can try it out in the settings window, just press the play button to see how a sumulated CH chain would look!{Environment.NewLine}" +
                         $"* All melee discipline cooldown timers will show in the cooldown section. No need to press a discipline button to see when you can use defensive!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 7),
                Message = $"* When a player dies, their spell effects will be removed from the Triggers window.{Environment.NewLine}" +
                          $"* When a player dies, they will no longer show up as death timer. Only NPC's will show in there now! {Environment.NewLine}" +
                          $"* Sort order of triggers window is now: Custom timers; You; NPC's, then players! {Environment.NewLine}" +
                          $"      This should help on raids because the previous sort order was: Custom Timers; You; then everything else alphabetically. {Environment.NewLine}" +
                          $"      So, raid targets would get lost in a massive list. Now they will appear near the top always!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 7),
                Message = $"* CH Overlay complete!. Try it out by checking the box in settings overlay and make sure the CH macro follows the format.{Environment.NewLine}" +
                 $"* Below is an example, of the look of the chain!{Environment.NewLine}",
                Image = "pack://application:,,,/update1.png",
                ImageVisibility = Visibility.Visible
            });
        }
    }
}
