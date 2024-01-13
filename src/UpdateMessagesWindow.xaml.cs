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
                DateTime = new DateTime(2024, 1, 7),
                Message = $"* Added a CH test button so you can try it out in the settings window, just press the play button to see how a sumulated CH chain would look!{Environment.NewLine}" +
               $"* All melee discipline cooldown timers will show in the cooldown section. No need to press a discipline button to see when you can use defensive!{Environment.NewLine}" +
               $"* Raid channel will pick up CH Chain messages. Added Examples to the settings window CH Chain format. {Environment.NewLine}" +
               $"* Added another CH Chain format support for those who don't want to use the TAG to differentiate their chain. Be aware, if you aren't using the TAG filter, you will see everyone's chain!!"
            });
            UpdateMessages.Add(new UpdateMessageData
            {
                DateTime = new DateTime(2024, 1, 13),
                Message = $"* Group invite Audio/Visual Triggers.{Environment.NewLine}" +
                     $"* Fixed FTE test button so it works in all zones now!"
            });
        }
    }
}
