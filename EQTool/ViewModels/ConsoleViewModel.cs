using EQTool.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace EQTool.ViewModels
{
    public class ConsoleLine
    {
        public string Line { get; set; }
        public Brush Brush { get; set; }

    }
    public class ConsoleViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ConsoleLine> ConsoleOutput { get; set; } = new ObservableCollection<ConsoleLine>();
        private readonly int MaxLines = 1000;
        private readonly IAppDispatcher appDispatcher;
        public ConsoleViewModel(IAppDispatcher appDispatcher)
        {
            this.appDispatcher = appDispatcher;
        }

        public void WriteLine(string message, Brush brush)
        {
            appDispatcher.DispatchUI(() =>
            {
                ConsoleOutput.Add(new ConsoleLine { Line = message, Brush = brush });
                while (ConsoleOutput.Count > MaxLines)
                {
                    ConsoleOutput.RemoveAt(0);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
