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
                DateTime = new DateTime(2024, 1, 15),
                Message = $"* Added an error when PigParse is ran from the Everquest folder. Running from the everquest folder is not supprted! {Environment.NewLine}" +
                  $"* Various bug fixes. {Environment.NewLine}" +
                  $"* Expanded CH chain formats. Now, the CH Chain is much more flexible and works with almost any format thrown at it. {Environment.NewLine}" +
                  $"* For example, the following are examples now supported: {Environment.NewLine}" +
                  $"*     Windarie auctions, '111 --- CH << Mandair  >> --- 111' {Environment.NewLine}" +
                  $"*     Kaijai auctions, '888 CH <<< Mandair >>> 888' {Environment.NewLine}" +
                  $"*     Mutao auctions, '777 CH <>> Mandair <<> 777' {Environment.NewLine}"
            });

            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 16),
                Message = $"* Added code so that updates will ONLY happen when you are idle for 10 minutes and have NO spells/timers running. I realize that the constant updates are annoying, and I am trying to find a balance! {Environment.NewLine}" +
                   $"* Updated the look of the CH Chain overlay after receiving feedback. {Environment.NewLine}" +
                   $"*    Basically, stole the colors from CCHP to make people feel more comfortable. {Environment.NewLine}" +
                   $"*    Fixed issue where the target names were taking up different amounts of space if their names were long. {Environment.NewLine}"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 16),
                Message = $"* Added more mob zone respawn times to various zones. {Environment.NewLine}" +
                   $"* Expanded CH chain formats. {Environment.NewLine}" +
                  $"* For example, the following are examples now supported: {Environment.NewLine}" +
                  $"*     Windarie auctions, 'AAA --- CH << Mandair  >> --- AAA' {Environment.NewLine}" +
                  $"*     Kaijai auctions, 'BBB CH <<< Mandair >>> BBB' {Environment.NewLine}"
            });
        }
    }
}
