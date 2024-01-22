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
                DateTime = new DateTime(2024, 1, 21),
                Message = $"* Fixed CH chain overlay where multiple chains overlapped each other. {Environment.NewLine}" +
                $"* Sharpened map lines. {Environment.NewLine}" +
                $"* Fixed names in maps from doubling up in listing! {Environment.NewLine}" +
                $"* Fixed slain timers from not showing up correctly! {Environment.NewLine}" +
                $"* Fixed Detrimental spell effects that were not showing up correctly when ShowOnlyYou setting enabled! {Environment.NewLine}"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 21),
                Message = $"* Brightened up the map lines. They were too dark before, now they should pop more! {Environment.NewLine}" +
                  $"* Added support for NParse map BI-Directional location sharing. Users of NParse will show in the list with (NP) next to their names. In NParse, PigParse users will have (PP) after their names. {Environment.NewLine}" +
                  $"* If your setting in PigParse is set to GuildOnly on location sharing, your setting will be respected and not be sent to NParse. If set to everyone, it will be shared! {Environment.NewLine}" +
                  $"* If you find any issues with the CH warning or the NParse map location sharing, please post on github! {Environment.NewLine}",
                Image = "pack://application:,,,/update1.png",
                ImageVisibility = Visibility.Visible
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 20),
                Message = $"* Added a CH Visual/Audio warning. This will alert you when the PERSON IN FRONT OF YOU goes so you can be ready! {Environment.NewLine}" +
                      $"* Fixed issue where some enraged mobs didnt trigger overlay/audio alerts. {Environment.NewLine}" +
                      $"* Discipline cooldown timers now show under the person, just as spells do. You can filter these in the same way. {Environment.NewLine}"
            });
        }
    }
}
