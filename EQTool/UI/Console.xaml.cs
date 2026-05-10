using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;

namespace EQTool.UI
{
    public partial class Console : BaseSaveStateWindow
    {
        public Console(ConsoleViewModel consoleViewModel, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad)
              : base(settings.ConsoleWindowState, toolSettingsLoad, settings)
        {
            DataContext = consoleViewModel;
            InitializeComponent();
            base.Init();
            consoleViewModel.ConsoleOutput.CollectionChanged += ConsoleOutput_CollectionChanged;
        }

        private void ConsoleOutput_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ConsoleScroller.ScrollToBottom();
        }
    }
}
