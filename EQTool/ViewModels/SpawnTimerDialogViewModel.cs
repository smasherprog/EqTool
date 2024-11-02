using EQTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace EQTool.ViewModels
{
    public class SpawnTimerDialogViewModel : INotifyPropertyChanged
    {
        private bool _expMessageSelected = false;

        public bool ExpMessageSelected
        {
            get { return _expMessageSelected; }
            set
            {
                this._expMessageSelected = value;
                SlainMessage = "Disabled";
                SlainMessageColor = Brushes.Red; 
                this.OnPropertyChanged(nameof(SlainMessageColor));
                this.OnPropertyChanged(); 
            }
        }

        private string _slainMessage = string.Empty;

        public string SlainMessage
        {
            get { return _slainMessage; }
            set
            {
                this._slainMessage = value;
                this.OnPropertyChanged();
            }
        }

        private bool _slainMessageSelected = false;

        public bool SlainMessageSelected
        {
            get { return _slainMessageSelected; }
            set
            {
                this._slainMessageSelected = value;
                SlainMessageColor = Brushes.Blue;
                this.SlainMessage = "Enter your message here";
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(SlainMessageColor));
            }
        }

        public SolidColorBrush SlainMessageColor { get; set; } = Brushes.Black;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void DoSomething_DontTryTopassData_ItShouldBeInTheeViewModelAllready()
        {
            //do something here with the button click. Maybe get the value of the text box and do an api call?
            var slainmessage = this.SlainMessage;

        }
    }
}
