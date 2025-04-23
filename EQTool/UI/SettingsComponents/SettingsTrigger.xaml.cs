using EQTool.ViewModels.SettingsComponents;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    /// <summary>
    /// Interaction logic for SettingsPlayer.xaml
    /// </summary>
    public partial class SettingsTrigger : UserControl
    {
        public SettingsTrigger(TreeTrigger treePlayer)
        {
            DataContext = new SettingsTriggerViewModel(treePlayer);
            InitializeComponent();
        }
    }
}
