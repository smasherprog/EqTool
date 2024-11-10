using EQTool.ViewModels;
using EQTool.ViewModels.SettingsComponents;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    /// <summary>
    /// Interaction logic for SettingsServer.xaml
    /// </summary>
    public partial class SettingsServer : UserControl
    {
        public SettingsServer(TreeServer treeServer)
        {
            DataContext = new SettingsServerViewModel(treeServer);
            InitializeComponent();
        }
    }
}
