using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels.SettingsComponents;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EQTool.UI
{
    public partial class SettingManagement : BaseSaveStateWindow
    {
        private readonly SettingsManagementViewModel settingsManagementViewModel;
        public SettingManagement(
            SettingsManagementViewModel settingsManagementViewModel,
            EQToolSettingsLoad toolSettingsLoad,
            EQToolSettings settings) : base(settings.SettingsWindowState, toolSettingsLoad, settings)
        {
            DataContext = this.settingsManagementViewModel = settingsManagementViewModel;
            InitializeComponent();
            base.Init();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            settingsManagementViewModel.TreeSelected(e.NewValue as TreeViewItemBase);
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = e.OriginalSource as DependencyObject;
            var item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            var p = (TreeViewItemBase)item.Header;
            if (p == null)
            {
                return;
            }

            _ = item.Focus();
            e.Handled = true;

            (sender as TreeViewItem).ContextMenu = settingsManagementViewModel.GetContextMenu(p);
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                {
                    break;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }
    }
}

