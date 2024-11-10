using EQTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EQTool.UI
{
    public partial class SettingManagement : Window
    {
        private readonly SettingsManagementViewModel settingsManagementViewModel;
        public SettingManagement(SettingsManagementViewModel settingsManagementViewModel)
        {
            DataContext = this.settingsManagementViewModel = settingsManagementViewModel;
            InitializeComponent();
        }

        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                _ = treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            settingsManagementViewModel.TreeSelected(e.NewValue as TreeViewItemBase);
        }
    }
}
