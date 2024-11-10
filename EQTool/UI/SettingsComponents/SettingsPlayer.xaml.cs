using EQTool.ViewModels;
using EQTool.ViewModels.SettingsComponents;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    /// <summary>
    /// Interaction logic for SettingsPlayer.xaml
    /// </summary>
    public partial class SettingsPlayer : UserControl
    {
        public SettingsPlayer(TreePlayer treePlayer)
        {
            DataContext = new SettingsPlayerViewModel(treePlayer);
            InitializeComponent();
        }
    }
}
