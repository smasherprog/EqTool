using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.SettingsComponents
{
    public class SettingsGeneralViewModel : INotifyPropertyChanged
    {
        public SettingsGeneralViewModel(TreePlayer treePlayer)
        {
            TreePlayer = treePlayer;
        }

        public TreePlayer TreePlayer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
