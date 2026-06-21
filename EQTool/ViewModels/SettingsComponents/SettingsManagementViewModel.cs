using EQTool.Models;
using EQTool.Services;

using EQToolShared.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace EQTool.ViewModels.SettingsComponents
{
    public class SettingsManagementViewModel : INotifyPropertyChanged
    {
        private readonly UserComponentSettingsManagementFactory userComponentFactory;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad eQToolSettingsLoad;
        private TreeGlobal triggersRoot;
        // The node that was Cut/Copied and is waiting to be Pasted (folder or trigger).
        private TreeViewItemBase clipboardNode;
        // True when the clipboard node should be duplicated on paste (Copy), false to move (Cut).
        private bool clipboardIsCopy;
        public SettingsManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory, EQToolSettings settings, EQToolSettingsLoad eQToolSettingsLoad)
        {
            this.userComponentFactory = userComponentFactory;
            this.settings = settings;
            this.eQToolSettingsLoad = eQToolSettingsLoad;
            _TreeItems.Add(new TreeGeneral("General", null)
            {
                IsSelected = true
            });

            triggersRoot = new TreeGlobal("Triggers", null);
            _TreeItems.Add(triggersRoot);
            BuildTriggerTree();

            foreach (var item in Enum.GetValues(typeof(Servers)).Cast<Servers>().Where(a => a != Servers.MaxServers && a != Servers.Quarm).ToList())
            {
                var players = settings.Players.Where(a => a.Server == item).ToList();
                var treeServer = new TreeServer(item.ToString(), null);
                _TreeItems.Add(treeServer);
                treeServer.Children.Add(new TreeZone("Zone(s)", null));
                foreach (var p in players.OrderBy(a => a.Name))
                {
                    treeServer.Children.Add(new TreePlayer(treeServer)
                    {
                        Player = p
                    });
                }
            }
            UserControl = this.userComponentFactory.CreateComponent(TreeViewItemType.General);

        }

        // Rebuilds the Triggers branch (folders + triggers) from the flat lists
        // stored in settings. Folders are linked to their parents via ParentId and
        // triggers are placed in their FolderId folder (or the root when null).
        private void BuildTriggerTree()
        {
            triggersRoot.Children.Clear();
            BuildBuiltInCategory();
            var folderNodes = new System.Collections.Generic.Dictionary<Guid, TreeTriggerFolder>();
            foreach (var f in settings.TriggerFolders)
            {
                folderNodes[f.Id] = new TreeTriggerFolder(f, triggersRoot);
            }

            foreach (var f in settings.TriggerFolders)
            {
                var node = folderNodes[f.Id];
                if (f.ParentId.HasValue && folderNodes.TryGetValue(f.ParentId.Value, out var parentNode))
                {
                    node.Parent = parentNode;
                    parentNode.Children.Add(node);
                }
                else
                {
                    node.Parent = triggersRoot;
                    triggersRoot.Children.Add(node);
                }
            }

            foreach (var trigger in settings.Triggers)
            {
                TreeViewItemBase parent = triggersRoot;
                if (trigger.FolderId.HasValue && folderNodes.TryGetValue(trigger.FolderId.Value, out var fnode))
                {
                    parent = fnode;
                }
                parent.Children.Add(new TreeTrigger(new TriggerViewModel(trigger, settings, eQToolSettingsLoad), parent));
            }
        }

        // Adds the read-only "Built In" library category and its triggers. This is
        // created in code each session and is excluded from persistence.
        private void BuildBuiltInCategory()
        {
            var folder = new TreeTriggerFolder(new TriggerFolder { Name = BuiltInTriggers.CategoryName }, triggersRoot)
            {
                IsBuiltIn = true
            };
            triggersRoot.Children.Add(folder);
            foreach (var t in BuiltInTriggers.All())
            {
                folder.Children.Add(new TreeTrigger(new TriggerViewModel(t, settings, eQToolSettingsLoad), folder));
            }
        }

        private MenuItem BuildMenuItem(string header, RoutedEventHandler handler, TreeViewItemBase tag)
        {
            var menuItem = new MenuItem { Header = header, Tag = tag };
            menuItem.Click += handler;
            return menuItem;
        }

        public ContextMenu GetContextMenu(TreeViewItemBase item)
        {
            if (item is TreeServer)
            {
                //no menu yet
                return null;
            }
            else if (item is TreeGlobal)
            {
                // The root of the Triggers branch.
                var menu = new ContextMenu();
                _ = menu.Items.Add(BuildMenuItem("Add Trigger", AddTrigger, item));
                _ = menu.Items.Add(BuildMenuItem("Add Folder", AddFolder, item));
                if (clipboardNode != null)
                {
                    _ = menu.Items.Add(BuildMenuItem("Paste", PasteItem, item));
                }
                return menu;
            }
            else if (item is TreeTriggerFolder folder)
            {
                var menu = new ContextMenu();
                if (folder.IsBuiltIn)
                {
                    // The Built In category can only be copied out of.
                    _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                    return menu;
                }
                _ = menu.Items.Add(BuildMenuItem("Add Trigger", AddTrigger, item));
                _ = menu.Items.Add(BuildMenuItem("Add Folder", AddFolder, item));
                _ = menu.Items.Add(BuildMenuItem("Rename", RenameItem, item));
                _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                _ = menu.Items.Add(BuildMenuItem("Cut", CutItem, item));
                if (clipboardNode != null)
                {
                    _ = menu.Items.Add(BuildMenuItem("Paste", PasteItem, item));
                }
                _ = menu.Items.Add(BuildMenuItem("Delete", DeleteFolder, item));
                return menu;
            }
            else if (item is TreeTrigger trig)
            {
                var menu = new ContextMenu();
                if (trig.IsBuiltIn)
                {
                    // Built In triggers can only be copied out of the library.
                    _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                    return menu;
                }
                _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                _ = menu.Items.Add(BuildMenuItem("Cut", CutItem, item));
                _ = menu.Items.Add(BuildMenuItem("Delete Trigger", DeleteTrigger, item));
                return menu;
            }
            else if (item is TreePlayer)
            {
                var menu = new ContextMenu();
                _ = menu.Items.Add(BuildMenuItem("Delete Saved Data", PlayerDelete, item));
                return menu;
            }

            return null;
        }

        private void AddTrigger(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase parent && (parent is TreeGlobal || parent is TreeTriggerFolder))
            {
                var vm = new TriggerViewModel(settings, eQToolSettingsLoad)
                {
                    FolderId = (parent as TreeTriggerFolder)?.Backing.Id
                };
                var newtrigger = new TreeTrigger(vm, parent) { IsSelected = true };
                parent.Children.Insert(0, newtrigger);
                parent.IsExpanded = true;
            }
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase parent && (parent is TreeGlobal || parent is TreeTriggerFolder))
            {
                var backing = new TriggerFolder
                {
                    Name = "New Folder",
                    ParentId = (parent as TreeTriggerFolder)?.Backing.Id
                };
                var node = new TreeTriggerFolder(backing, parent);
                parent.Children.Insert(0, node);
                parent.IsExpanded = true;
                node.IsSelected = true;
                PersistTriggerTree();
                node.BeginEdit();
            }
        }

        private void RenameItem(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeTriggerFolder folder)
            {
                folder.BeginEdit();
            }
        }

        private void CutItem(object sender, RoutedEventArgs e)
        {
            clipboardNode = (sender as MenuItem)?.Tag as TreeViewItemBase;
            clipboardIsCopy = false;
        }

        private void CopyItem(object sender, RoutedEventArgs e)
        {
            clipboardNode = (sender as MenuItem)?.Tag as TreeViewItemBase;
            clipboardIsCopy = true;
        }

        private void PasteItem(object sender, RoutedEventArgs e)
        {
            if (clipboardNode == null)
            {
                return;
            }
            if (!((sender as MenuItem)?.Tag is TreeViewItemBase target) || !(target is TreeGlobal || target is TreeTriggerFolder))
            {
                return;
            }
            // Never allow pasting into the read-only Built In library.
            if (target is TreeTriggerFolder tf && tf.IsBuiltIn)
            {
                return;
            }

            if (clipboardIsCopy)
            {
                // Duplicate the node (and its subtree) with fresh ids; the clone is
                // always editable even when copied out of the Built In library.
                var clone = CloneNode(clipboardNode, target);
                if (clone == null)
                {
                    return;
                }
                target.Children.Insert(0, clone);
                target.IsExpanded = true;
                AddTriggerModels(clone);
                PersistTriggerTree();
                return;
            }

            // Move (Cut): can't paste a node into itself or one of its own descendants.
            if (IsSelfOrDescendant(target, clipboardNode))
            {
                clipboardNode = null;
                return;
            }
            _ = clipboardNode.Parent?.Children.Remove(clipboardNode);
            clipboardNode.Parent = target;
            target.Children.Insert(0, clipboardNode);
            target.IsExpanded = true;
            clipboardNode = null;
            PersistTriggerTree();
        }

        // Deep-clones a tree node (trigger or folder + subtree) with new ids. The clone
        // is always a normal, editable node (built-in status is dropped).
        private TreeViewItemBase CloneNode(TreeViewItemBase node, TreeViewItemBase parent)
        {
            if (node is TreeTrigger tt)
            {
                var clone = CloneTrigger(tt.Trigger.Model);
                return new TreeTrigger(new TriggerViewModel(clone, settings, eQToolSettingsLoad), parent);
            }
            if (node is TreeTriggerFolder tf)
            {
                var backing = new TriggerFolder { Id = Guid.NewGuid(), Name = tf.Backing.Name };
                var folderNode = new TreeTriggerFolder(backing, parent);
                foreach (var child in tf.Children)
                {
                    var childClone = CloneNode(child, folderNode);
                    if (childClone != null)
                    {
                        folderNode.Children.Add(childClone);
                    }
                }
                return folderNode;
            }
            return null;
        }

        // JSON round-trip deep copy of a trigger with a fresh id. IsBuiltIn is JsonIgnore,
        // so the clone is never marked built-in.
        private Models.Trigger CloneTrigger(Models.Trigger source)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var clone = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Trigger>(json);
            clone.TriggerId = Guid.NewGuid();
            clone.FolderId = null;
            return clone;
        }

        // Registers the cloned trigger model(s) in settings so they persist.
        private void AddTriggerModels(TreeViewItemBase node)
        {
            if (node is TreeTrigger tt)
            {
                settings.Triggers.Add(tt.Trigger.Model);
            }
            else
            {
                foreach (var child in node.Children)
                {
                    AddTriggerModels(child);
                }
            }
        }

        // Returns true if 'candidate' is 'node' itself or anywhere in 'node's subtree.
        private bool IsSelfOrDescendant(TreeViewItemBase candidate, TreeViewItemBase node)
        {
            var current = candidate;
            while (current != null)
            {
                if (current == node)
                {
                    return true;
                }
                current = current.Parent;
            }
            return false;
        }

        private void DeleteFolder(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeTriggerFolder t)
            {
                var result = System.Windows.MessageBox.Show($"Are you sure that you want to delete the folder '{t.Name}' and everything inside it?", $"Delete Folder '{t.Name}'", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    RemoveTriggersUnder(t);
                    if (clipboardNode != null && IsSelfOrDescendant(clipboardNode, t))
                    {
                        clipboardNode = null;
                    }
                    _ = t.Parent.Children.Remove(t);
                    PersistTriggerTree();
                }
            }
        }

        // Removes the underlying Trigger of every TreeTrigger in this subtree from settings.Triggers.
        private void RemoveTriggersUnder(TreeViewItemBase node)
        {
            foreach (var child in node.Children)
            {
                if (child is TreeTrigger tt)
                {
                    _ = settings.Triggers.RemoveAll(a => a.TriggerId == tt.Trigger.TriggerId);
                }
                else
                {
                    RemoveTriggersUnder(child);
                }
            }
        }

        private void DeleteTrigger(object sender, RoutedEventArgs e)
        {
            var s = sender as MenuItem;
            if (s.Tag is TreeTrigger t)
            {
                var result = System.Windows.MessageBox.Show($"Are you sure that you want to delete the trigger '{t.Name}'?", $"Delete Trigger '{t.Name}'", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = settings.Triggers.RemoveAll(a => a.TriggerId == t.Trigger.TriggerId);
                    if (clipboardNode == t)
                    {
                        clipboardNode = null;
                    }
                    _ = t.Parent.Children.Remove(t);
                    eQToolSettingsLoad.Save(settings);
                }
            }
        }

        // Called when inline editing of a node's name finishes (Enter / focus lost).
        public void CommitEdit(TreeViewItemBase node)
        {
            if (node == null || !node.IsEditing)
            {
                return;
            }
            node.IsEditing = false;
            if (node is TreeTriggerFolder)
            {
                PersistTriggerTree();
            }
        }

        // Walks the current tree and writes the folder hierarchy and each trigger's
        // FolderId back into settings, then saves. This keeps the persisted flat
        // lists in sync with the tree after any structural change.
        private void PersistTriggerTree()
        {
            var folders = new System.Collections.Generic.List<TriggerFolder>();
            WalkAndCollect(triggersRoot, null, folders);
            settings.TriggerFolders = folders;
            eQToolSettingsLoad.Save(settings);
        }

        private void WalkAndCollect(TreeViewItemBase node, Guid? parentId, System.Collections.Generic.List<TriggerFolder> folders)
        {
            foreach (var child in node.Children)
            {
                if (child is TreeTriggerFolder f)
                {
                    // The Built In library is created in code and never persisted.
                    if (f.IsBuiltIn)
                    {
                        continue;
                    }
                    f.Backing.ParentId = parentId;
                    folders.Add(f.Backing);
                    WalkAndCollect(f, f.Backing.Id, folders);
                }
                else if (child is TreeTrigger t)
                {
                    if (t.IsBuiltIn)
                    {
                        continue;
                    }
                    t.Trigger.FolderId = parentId;
                }
            }
        }

        private void PlayerDelete(object sender, System.Windows.RoutedEventArgs e)
        {
            var s = sender as MenuItem;
            if (s.Tag is TreePlayer t)
            {
                var result = System.Windows.MessageBox.Show($"Are you sure that you want to delete the saved settings for {t.Name}? This only deletes Pigparse data!", $"Delete Pigparse data for {t.Name}", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = settings.Players.Remove(t.Player);
                    eQToolSettingsLoad.Save(settings);
                    _ = t.Parent.Children.Remove(t);
                }
            }
        }

        private ObservableCollection<TreeViewItemBase> _TreeItems = new ObservableCollection<TreeViewItemBase>();
        public ObservableCollection<TreeViewItemBase> TreeItems
        {
            get => _TreeItems;
            set
            {
                if (value != _TreeItems)
                {
                    _TreeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        private UserControl _userControl;

        public UserControl UserControl
        {
            get => _userControl;
            set
            {
                _userControl = value;
                OnPropertyChanged();
            }
        }

        public void TreeSelected(TreeViewItemBase p)
        {
            UserControl = userComponentFactory.CreateComponent(p.Type, p);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
