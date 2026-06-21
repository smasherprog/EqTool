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
            // A plain (non-Ctrl) selection collapses the multi-selection down to the
            // single newly-selected item, keeping the highlight consistent.
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                ClearMultiSelect();
                if (e.NewValue is TreeViewItemBase selected)
                {
                    selected.IsMultiSelected = true;
                }
            }
            settingsManagementViewModel.TreeSelected(e.NewValue as TreeViewItemBase);
        }

        // Ctrl+click toggles an item's membership in the multi-selection; a plain
        // click clears the multi-selection and falls through to normal single-select.
        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = e.OriginalSource as DependencyObject;
            // let the expand/collapse toggle work normally
            if (IsInExpander(obj))
            {
                return;
            }

            var container = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            if (!(container?.Header is TreeViewItemBase data))
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // toggle this item in/out of the selection without changing the
                // primary selection (so the editor panel stays put)
                if (data.IsMultiSelected)
                {
                    // removing from the selection is always allowed
                    data.IsMultiSelected = false;
                }
                else
                {
                    // a multi-selection is all built-in or all non-built-in: only allow
                    // adding an item that matches the current selection's kind.
                    var anchor = FirstMultiSelected();
                    if (anchor == null || IsBuiltInNode(anchor) == IsBuiltInNode(data))
                    {
                        data.IsMultiSelected = true;
                    }
                }
                e.Handled = true;
            }
            // a non-Ctrl click is handled by TreeView_SelectedItemChanged
        }

        private static bool IsBuiltInNode(TreeViewItemBase node)
        {
            return (node is TreeTriggerFolder f && f.IsBuiltIn) || (node is TreeTrigger t && t.IsBuiltIn);
        }

        private TreeViewItemBase FirstMultiSelected()
        {
            foreach (var item in settingsManagementViewModel.TreeItems)
            {
                var found = FindMultiSelected(item);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static TreeViewItemBase FindMultiSelected(TreeViewItemBase node)
        {
            if (node.IsMultiSelected)
            {
                return node;
            }
            foreach (var child in node.Children)
            {
                var found = FindMultiSelected(child);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private void ClearMultiSelect()
        {
            foreach (var item in settingsManagementViewModel.TreeItems)
            {
                ClearMultiSelectRecursive(item);
            }
        }

        private static void ClearMultiSelectRecursive(TreeViewItemBase node)
        {
            node.IsMultiSelected = false;
            foreach (var child in node.Children)
            {
                ClearMultiSelectRecursive(child);
            }
        }

        private static bool IsInExpander(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is System.Windows.Controls.Primitives.ToggleButton)
                {
                    return true;
                }
                if (obj is TreeViewItem)
                {
                    return false;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return false;
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

