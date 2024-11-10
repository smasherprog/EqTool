using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.SettingsComponents
{
    public class SettingsServerViewModel : INotifyPropertyChanged
    {
        public SettingsServerViewModel(TreeServer treeServer)
        {
            TreeServer = treeServer;
        }

        public TreeServer TreeServer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
