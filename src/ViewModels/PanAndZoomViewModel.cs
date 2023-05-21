using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EQTool.ViewModels
{
    public class PanAndZoomViewModel : INotifyPropertyChanged
    {
        private readonly UIElementCollection Children;

        public PanAndZoomViewModel(UIElementCollection collection)
        {
            Children = collection;
        }

        private string _TimerValue = "72";
        public string TimerValue
        {
            get => _TimerValue;
            set
            {
                _TimerValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
