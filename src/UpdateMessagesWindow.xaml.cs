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
                DateTime = new DateTime(2024, 1, 1),
                Message = $"*Butcher block npc timers updated.{Environment.NewLine}" +
                $"*Work on an overlay started. Currently, just enrage works, but there will be a CH chain visualization, charm break, fte, etc.{Environment.NewLine}" +
                $"*Settings window is changing so the sections are better organized. There will be more options in the future!{Environment.NewLine}" +
                $"*If you have any issues or suggestions, please goto https://github.com/smasherprog/EqTool and post in the issues section!{Environment.NewLine}" +
                $"*The following counters have been added:{Environment.NewLine}" +
                $"     -LowerElement (Flux Staff Effect) {Environment.NewLine}" +
                $"     -Concussion {Environment.NewLine}" +
                $"     -Flame Lick {Environment.NewLine}" +
                $"     -Cinder Jolt {Environment.NewLine}" +
                $"     -Jolt {Environment.NewLine}",
                Image = "pack://application:,,,/update1.png",
                ImageVisibility = Visibility.Visible
            });
        }
    }
}
