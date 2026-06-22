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
        // The node(s) that were Cut/Copied and are waiting to be Pasted (folders/triggers).
        private readonly System.Collections.Generic.List<TreeViewItemBase> clipboardNodes = new System.Collections.Generic.List<TreeViewItemBase>();
        // True when the clipboard nodes should be duplicated on paste (Copy), false to move (Cut).
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
                parent.Children.Add(NewTriggerNode(new TriggerViewModel(trigger, settings, eQToolSettingsLoad), parent));
            }

            SortRecursive(triggersRoot);
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
                folder.Children.Add(NewTriggerNode(new TriggerViewModel(t, settings, eQToolSettingsLoad), folder));
            }
        }

        // Adds a node and re-sorts the parent so children stay alphabetical.
        private void InsertChild(TreeViewItemBase parent, TreeViewItemBase node)
        {
            parent.Children.Add(node);
            SortChildren(parent);
        }

        // Sorts a parent's children alphabetically by Name, always keeping the read-only
        // Built In category pinned as the first item under the Triggers root.
        private void SortChildren(TreeViewItemBase parent)
        {
            TreeViewItemBase pinned = null;
            if (parent == triggersRoot && parent.Children.Count > 0 &&
                parent.Children[0] is TreeTriggerFolder first && first.IsBuiltIn)
            {
                pinned = parent.Children[0];
            }

            var ordered = parent.Children
                .Where(c => c != pinned)
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            parent.Children.Clear();
            if (pinned != null)
            {
                parent.Children.Add(pinned);
            }
            foreach (var c in ordered)
            {
                parent.Children.Add(c);
            }
        }

        private void SortRecursive(TreeViewItemBase node)
        {
            SortChildren(node);
            foreach (var child in node.Children)
            {
                SortRecursive(child);
            }
        }

        // Creates a trigger node and keeps the tree sorted when its name changes.
        private TreeTrigger NewTriggerNode(TriggerViewModel vm, TreeViewItemBase parent)
        {
            var node = new TreeTrigger(vm, parent);
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TriggerViewModel.TriggerName) && node.Parent != null)
                {
                    SortChildren(node.Parent);
                }
            };
            return node;
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
                if (clipboardNodes.Count > 0)
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
                    // The Built In category can only be enabled (copied out + enabled).
                    _ = menu.Items.Add(BuildMenuItem("Enable", EnableBuiltIn, item));
                    return menu;
                }
                _ = menu.Items.Add(BuildMenuItem("Add Trigger", AddTrigger, item));
                _ = menu.Items.Add(BuildMenuItem("Add Folder", AddFolder, item));
                _ = menu.Items.Add(BuildMenuItem("Rename", RenameItem, item));
                _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                _ = menu.Items.Add(BuildMenuItem("Cut", CutItem, item));
                if (clipboardNodes.Count > 0)
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
                    // Built In triggers can only be enabled (copied out + enabled).
                    _ = menu.Items.Add(BuildMenuItem("Enable", EnableBuiltIn, item));
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
                var newtrigger = NewTriggerNode(vm, parent);
                newtrigger.IsSelected = true;
                InsertChild(parent, newtrigger);
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
                InsertChild(parent, node);
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
            if (!((sender as MenuItem)?.Tag is TreeViewItemBase clicked))
            {
                return;
            }
            // Built-in library items can't be moved out.
            clipboardNodes.Clear();
            clipboardNodes.AddRange(ResolveSelection(clicked).Where(n => !IsBuiltInNode(n)));
            clipboardIsCopy = false;
        }

        private void CopyItem(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is TreeViewItemBase clicked))
            {
                return;
            }
            clipboardNodes.Clear();
            clipboardNodes.AddRange(ResolveSelection(clicked));
            clipboardIsCopy = true;
        }

        // Copies the selected Built In trigger(s) into the editable Triggers section and
        // enables them. Skips any that are already present - matched first by BuiltInId (which
        // survives copy/cut and rename) and otherwise by Trigger Name + Search Text.
        private void EnableBuiltIn(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is TreeViewItemBase clicked))
            {
                return;
            }

            var sources = new System.Collections.Generic.List<TreeTrigger>();
            foreach (var node in ResolveSelection(clicked))
            {
                CollectTriggerNodes(node, sources);
            }

            var added = false;
            foreach (var tt in sources)
            {
                var source = tt.Trigger.Model;
                var duplicate = settings.Triggers.Any(a =>
                    (!string.IsNullOrEmpty(source.BuiltInId) &&
                        string.Equals(a.BuiltInId, source.BuiltInId, StringComparison.OrdinalIgnoreCase)) ||
                    (string.Equals(a.TriggerName, source.TriggerName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(a.SearchText, source.SearchText, StringComparison.OrdinalIgnoreCase)));
                if (duplicate)
                {
                    continue;
                }

                var clone = CloneTrigger(source);
                clone.TriggerEnabled = true;
                settings.Triggers.Add(clone);
                var newNode = NewTriggerNode(new TriggerViewModel(clone, settings, eQToolSettingsLoad), triggersRoot);
                triggersRoot.Children.Add(newNode);
                added = true;
            }

            if (added)
            {
                triggersRoot.IsExpanded = true;
                SortChildren(triggersRoot);
                PersistTriggerTree();
            }
        }

        // Collects all trigger nodes within a subtree (the node itself if it is a trigger).
        private void CollectTriggerNodes(TreeViewItemBase node, System.Collections.Generic.List<TreeTrigger> acc)
        {
            if (node is TreeTrigger t)
            {
                acc.Add(t);
                return;
            }
            foreach (var child in node.Children)
            {
                CollectTriggerNodes(child, acc);
            }
        }

        // Determines which nodes an operation should act on: the full multi-selection
        // when the clicked item is part of it, otherwise just the clicked item. Nested
        // selections are reduced to their top-most nodes to avoid duplicate work.
        private System.Collections.Generic.List<TreeViewItemBase> ResolveSelection(TreeViewItemBase clicked)
        {
            var selected = new System.Collections.Generic.List<TreeViewItemBase>();
            CollectMultiSelected(triggersRoot, selected);
            if (selected.Count > 0 && selected.Contains(clicked))
            {
                return selected.Where(n => !HasSelectedAncestor(n, selected)).ToList();
            }
            return new System.Collections.Generic.List<TreeViewItemBase> { clicked };
        }

        private void CollectMultiSelected(TreeViewItemBase node, System.Collections.Generic.List<TreeViewItemBase> acc)
        {
            foreach (var child in node.Children)
            {
                if (child.IsMultiSelected)
                {
                    acc.Add(child);
                }
                CollectMultiSelected(child, acc);
            }
        }

        private bool HasSelectedAncestor(TreeViewItemBase node, System.Collections.Generic.List<TreeViewItemBase> set)
        {
            var p = node.Parent;
            while (p != null)
            {
                if (set.Contains(p))
                {
                    return true;
                }
                p = p.Parent;
            }
            return false;
        }

        private bool IsBuiltInNode(TreeViewItemBase node)
        {
            return (node is TreeTriggerFolder f && f.IsBuiltIn) || (node is TreeTrigger t && t.IsBuiltIn);
        }

        private void PasteItem(object sender, RoutedEventArgs e)
        {
            if (clipboardNodes.Count == 0)
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
                // Duplicate each node (and its subtree) with fresh ids; clones are
                // always editable even when copied out of the Built In library.
                foreach (var node in clipboardNodes)
                {
                    var clone = CloneNode(node, target);
                    if (clone == null)
                    {
                        continue;
                    }
                    target.Children.Add(clone);
                    AddTriggerModels(clone);
                }
                SortChildren(target);
                target.IsExpanded = true;
                PersistTriggerTree();
                return;
            }

            // Move (Cut): skip nodes that would be pasted into themselves/their subtree.
            foreach (var node in clipboardNodes.ToList())
            {
                if (IsSelfOrDescendant(target, node))
                {
                    continue;
                }
                _ = node.Parent?.Children.Remove(node);
                node.Parent = target;
                target.Children.Add(node);
            }
            SortChildren(target);
            target.IsExpanded = true;
            clipboardNodes.Clear();
            PersistTriggerTree();
        }

        // Deep-clones a tree node (trigger or folder + subtree) with new ids. The clone
        // is always a normal, editable node (built-in status is dropped).
        private TreeViewItemBase CloneNode(TreeViewItemBase node, TreeViewItemBase parent)
        {
            if (node is TreeTrigger tt)
            {
                var clone = CloneTrigger(tt.Trigger.Model);
                return NewTriggerNode(new TriggerViewModel(clone, settings, eQToolSettingsLoad), parent);
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
        // so the clone is never marked built-in. BuiltInId IS persisted and is deliberately
        // carried across so a copied built-in stays recognizable (for duplicate detection and
        // "already present" checks); only the per-instance TriggerId/FolderId are reset.
        private Models.Trigger CloneTrigger(Models.Trigger source)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var clone = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Trigger>(json);
            clone.TriggerId = Guid.NewGuid();
            clone.FolderId = null;
            clone.BuiltInId = source.BuiltInId;
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
            DeleteSelection((sender as MenuItem)?.Tag as TreeViewItemBase);
        }

        // Deletes the resolved selection (all multi-selected items when the clicked item
        // is part of it, otherwise just the clicked item). Built-in items are never deleted.
        private void DeleteSelection(TreeViewItemBase clicked)
        {
            if (clicked == null)
            {
                return;
            }
            var nodes = ResolveSelection(clicked).Where(n => !IsBuiltInNode(n)).ToList();
            if (nodes.Count == 0)
            {
                return;
            }

            var message = nodes.Count == 1
                ? $"Are you sure that you want to delete '{nodes[0].Name}'?" + (nodes[0] is TreeTriggerFolder ? " This deletes everything inside it." : string.Empty)
                : $"Are you sure that you want to delete the {nodes.Count} selected items? This deletes everything inside any selected folders.";
            var result = System.Windows.MessageBox.Show(message, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var node in nodes)
            {
                if (node is TreeTrigger tt)
                {
                    _ = settings.Triggers.RemoveAll(a => a.TriggerId == tt.Trigger.TriggerId);
                }
                else if (node is TreeTriggerFolder)
                {
                    RemoveTriggersUnder(node);
                }
                _ = clipboardNodes.RemoveAll(n => IsSelfOrDescendant(n, node));
                _ = node.Parent?.Children.Remove(node);
            }
            PersistTriggerTree();
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
            DeleteSelection((sender as MenuItem)?.Tag as TreeViewItemBase);
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
                if (node.Parent != null)
                {
                    SortChildren(node.Parent);
                }
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
