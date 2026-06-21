using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
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
            EQToolSettings settings,
            ConsoleViewModel consoleViewModel) : base(settings.SettingsWindowState, toolSettingsLoad, settings, consoleViewModel)
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

        private void RenameTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.IsVisible)
            {
                // Defer focus until after the context menu has fully closed and layout
                // has settled, otherwise the TextBox never receives keyboard focus.
                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    _ = tb.Focus();
                    _ = Keyboard.Focus(tb);
                    tb.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                if (tb?.DataContext is TreeViewItemBase node)
                {
                    settingsManagementViewModel.CommitEdit(node);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (tb?.DataContext is TreeTriggerFolder folder)
                {
                    folder.CancelEdit();
                }
                e.Handled = true;
            }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox)?.DataContext is TreeViewItemBase node)
            {
                settingsManagementViewModel.CommitEdit(node);
            }
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

