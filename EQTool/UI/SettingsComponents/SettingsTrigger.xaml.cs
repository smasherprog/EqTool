using EQTool.ViewModels.SettingsComponents;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    public partial class SettingsTrigger : UserControl
    {
        private readonly TreeTrigger treeTrigger;
        public SettingsTrigger(TreeTrigger treeTrigger)
        {
            this.treeTrigger = treeTrigger;
            DataContext = treeTrigger.Trigger;
            InitializeComponent();
        }

        private void Save(object sender, System.Windows.RoutedEventArgs e)
        {
            this.treeTrigger.Trigger.Save();
        }
    }
}
