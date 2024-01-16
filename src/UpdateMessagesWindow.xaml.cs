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
                DateTime = new DateTime(2024, 1, 14),
                Message =
                      $"* Added timers for AOE's for the following mobs: {Environment.NewLine}" +
                      $"*     Telkorenar {Environment.NewLine}" +
                      $"*     Gozzrem {Environment.NewLine}" +
                      $"*     Lendiniara the Keeper {Environment.NewLine}" +
                      $"*     Lady Mirenilla {Environment.NewLine}" +
                      $"*     Lord Koi'Doken {Environment.NewLine}" +
                      $"*     Zlexak {Environment.NewLine}" +
                      $"*     Hoshkar {Environment.NewLine}"
            });
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
        }
    }
}
