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
                DateTime = new DateTime(2024, 1, 8),
                Message = $"* Fixed an issue when parsing the log filename.{Environment.NewLine}" +
                 $"* Added more CH chain scenarios. Going to add many different formats so by the end most ch formats will just work."
             });

            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 7),
                Message = $"* Added a CH test button so you can try it out in the settings window, just press the play button to see how a sumulated CH chain would look!{Environment.NewLine}" +
                         $"* All melee discipline cooldown timers will show in the cooldown section. No need to press a discipline button to see when you can use defensive!{Environment.NewLine}" +
                         $"* Raid channel will pick up CH Chain messages. Added Examples to the settings window CH Chain format. {Environment.NewLine}" +
                         $"* Added another CH Chain format support for those who don't want to use the TAG to differentiate their chain. Be aware, if you aren't using the TAG filter, you will see everyone's chain!!"
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
