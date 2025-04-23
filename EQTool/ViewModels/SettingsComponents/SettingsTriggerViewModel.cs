using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.SettingsComponents
{
    public class SettingsTriggerViewModel : INotifyPropertyChanged
    {
        public SettingsTriggerViewModel(TreeTrigger treeTrigger)
        {
            TreeTrigger = treeTrigger;
        }

        public TreeTrigger TreeTrigger { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
