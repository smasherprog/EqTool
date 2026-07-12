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
        private readonly EQSpells eqSpells;
        private readonly TreeGlobal triggersRoot;
        // The node(s) that were Cut/Copied and are waiting to be Pasted (folders/triggers).
        private readonly System.Collections.Generic.List<TreeViewItemBase> clipboardNodes = new System.Collections.Generic.List<TreeViewItemBase>();
        // True when the clipboard nodes should be duplicated on paste (Copy), false to move (Cut).
        private bool clipboardIsCopy;
        public SettingsManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory, EQToolSettings settings, EQToolSettingsLoad eQToolSettingsLoad, EQSpells eqSpells)
        {
            this.userComponentFactory = userComponentFactory;
            this.settings = settings;
            this.eQToolSettingsLoad = eQToolSettingsLoad;
            this.eqSpells = eqSpells;

            triggersRoot = new TreeGlobal("Triggers", null);
            BuildTriggerTree();
            TriggerTreeItems = triggersRoot.Children;

            foreach (var item in Enum.GetValues(typeof(Servers)).Cast<Servers>().Where(a => a != Servers.MaxServers && a != Servers.Quarm).ToList())
            {
                var players = settings.Players.Where(a => a.Server == item).ToList();
                var treeServer = new TreeServer(item.ToString(), null);
                _characterTreeItems.Add(treeServer);
                treeServer.Children.Add(new TreeZone("Zone(s)", null));
                foreach (var p in players.OrderBy(a => a.Name))
                {
                    treeServer.Children.Add(new TreePlayer(treeServer)
                    {
                        Player = p
                    });
                }
            }
        }

        // Rebuilds the Triggers branch as a single merged tree from settings.Triggers. Built-in
        // triggers (IsBuiltIn, kept in the list by EQToolSettingsLoad.SyncBuiltInTriggers) are
        // read-only and placed into their declared "/"-separated folders; user triggers go into the
        // user's own folders (by FolderId). Both kinds carry their own TriggerEnabled.
        private void BuildTriggerTree()
        {
            triggersRoot.Children.Clear();

            // Read-only built-in folders are created on demand from "/"-separated paths (a
            // built-in trigger's declared folder, or the built-in path a user folder/trigger
            // is anchored to).
            var builtInFolderCache = new System.Collections.Generic.Dictionary<string, TreeTriggerFolder>(StringComparer.OrdinalIgnoreCase);

            // User folders (persisted), linked by ParentId (user parent) or BuiltInParentPath
            // (placed inside a built-in library folder).
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
                else if (!string.IsNullOrWhiteSpace(f.BuiltInParentPath))
                {
                    var builtInParent = GetOrCreateBuiltInFolder(triggersRoot, f.BuiltInParentPath, builtInFolderCache);
                    node.Parent = builtInParent;
                    builtInParent.Children.Add(node);
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
                if (trigger.IsBuiltIn)
                {
                    if (!string.IsNullOrWhiteSpace(trigger.BuiltInFolder))
                    {
                        parent = GetOrCreateBuiltInFolder(triggersRoot, trigger.BuiltInFolder, builtInFolderCache);
                    }
                }
                else if (trigger.FolderId.HasValue && folderNodes.TryGetValue(trigger.FolderId.Value, out var fnode))
                {
                    parent = fnode;
                }
                else if (!string.IsNullOrWhiteSpace(trigger.BuiltInFolderPath))
                {
                    // User trigger anchored inside a built-in library folder.
                    parent = GetOrCreateBuiltInFolder(triggersRoot, trigger.BuiltInFolderPath, builtInFolderCache);
                }
                parent.Children.Add(NewTriggerNode(new TriggerViewModel(trigger, settings, eQToolSettingsLoad, eqSpells), parent));
            }

            SortRecursive(triggersRoot);
        }

        // Clears every trigger and user folder, then re-seeds the built-in library from code exactly
        // as a brand-new user would get it (all built-ins present and enabled). This discards all
        // user-created triggers/folders and any customizations to built-ins, then persists and
        // rebuilds the tree. Used by the Debug tab's "Reset Triggers" button.
        public void ResetTriggersToDefault()
        {
            settings.Triggers = new System.Collections.Generic.List<Models.Trigger>();
            settings.TriggerFolders = new System.Collections.Generic.List<TriggerFolder>();
            _ = EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            eQToolSettingsLoad.Save(settings);
            BuildTriggerTree();
        }

        // Walks/creates the nested read-only built-in folder chain for a "/"-separated path, rooted
        // at the given node, reusing previously created folders for shared path segments.
        private TreeTriggerFolder GetOrCreateBuiltInFolder(TreeViewItemBase root, string path, System.Collections.Generic.Dictionary<string, TreeTriggerFolder> cache)
        {
            TreeViewItemBase current = root;
            TreeTriggerFolder leaf = null;
            var accumulated = string.Empty;
            foreach (var segment in path.Split('/'))
            {
                var name = segment.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                accumulated = accumulated.Length == 0 ? name : accumulated + "/" + name;
                if (!cache.TryGetValue(accumulated, out var next))
                {
                    next = new TreeTriggerFolder(new TriggerFolder { Name = name }, current)
                    {
                        IsBuiltIn = true
                    };
                    current.Children.Add(next);
                    cache[accumulated] = next;
                }
                current = next;
                leaf = next;
            }
            return leaf;
        }

        // Adds a node and re-sorts the parent so children stay alphabetical.
        private void InsertChild(TreeViewItemBase parent, TreeViewItemBase node)
        {
            parent.Children.Add(node);
            SortChildren(parent);
        }

        // Sorts a parent's children: folders first, then triggers, each alphabetical by Name.
        private void SortChildren(TreeViewItemBase parent)
        {
            var ordered = parent.Children
                .OrderBy(c => c is TreeTriggerFolder ? 0 : 1)
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            parent.Children.Clear();
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

        // Context menu for the (hidden) Triggers root, used when right-clicking empty space
        // in the Triggers tree so top-level folders/triggers can still be added or pasted.
        public ContextMenu GetTriggerRootContextMenu()
        {
            return GetContextMenu(triggersRoot);
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
                _ = menu.Items.Add(new Separator());
                _ = menu.Items.Add(BuildMenuItem("Expand All", ExpandAll, item));
                _ = menu.Items.Add(BuildMenuItem("Collapse All", CollapseAll, item));
                return menu;
            }
            else if (item is TreeTriggerFolder folder)
            {
                // Built-in folders themselves are read-only (no rename/cut/delete), but users can
                // add their own triggers and folders inside them, and paste into them.
                if (folder.IsBuiltIn)
                {
                    var builtinMenu = new ContextMenu();
                    _ = builtinMenu.Items.Add(BuildMenuItem("Add Trigger", AddTrigger, item));
                    _ = builtinMenu.Items.Add(BuildMenuItem("Add Folder", AddFolder, item));
                    if (clipboardNodes.Count > 0)
                    {
                        _ = builtinMenu.Items.Add(BuildMenuItem("Paste", PasteItem, item));
                    }
                    _ = builtinMenu.Items.Add(new Separator());
                    _ = builtinMenu.Items.Add(BuildMenuItem("Expand All", ExpandAll, item));
                    _ = builtinMenu.Items.Add(BuildMenuItem("Collapse All", CollapseAll, item));
                    return builtinMenu;
                }
                var menu = new ContextMenu();
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
                _ = menu.Items.Add(new Separator());
                _ = menu.Items.Add(BuildMenuItem("Expand All", ExpandAll, item));
                _ = menu.Items.Add(BuildMenuItem("Collapse All", CollapseAll, item));
                return menu;
            }
            else if (item is TreeTrigger trig)
            {
                var menu = new ContextMenu();
                if (trig.IsBuiltIn)
                {
                    // Built-in triggers can only be enabled/disabled or copied (into an editable copy).
                    _ = menu.Items.Add(BuildMenuItem(trig.Trigger.TriggerEnabled ? "Disable" : "Enable", ToggleTriggerEnabled, item));
                    _ = menu.Items.Add(BuildMenuItem("Copy", CopyItem, item));
                    return menu;
                }
                _ = menu.Items.Add(BuildMenuItem(trig.Trigger.TriggerEnabled ? "Disable" : "Enable", ToggleTriggerEnabled, item));
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

        // Expands the clicked folder (or the whole tree from the root) and every folder beneath it.
        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase node)
            {
                SetExpandedRecursive(node, true);
            }
        }

        // Collapses the clicked folder (or the whole tree from the root) and every folder beneath it.
        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase node)
            {
                SetExpandedRecursive(node, false);
            }
        }

        // Sets IsExpanded on a node and its entire subtree. The hidden triggersRoot is never a
        // visible TreeViewItem, so only its children are touched when starting from the root.
        private void SetExpandedRecursive(TreeViewItemBase node, bool expanded)
        {
            if (!(node is TreeGlobal))
            {
                node.IsExpanded = expanded;
            }
            foreach (var child in node.Children)
            {
                SetExpandedRecursive(child, expanded);
            }
        }

        // The "/"-separated library path of a built-in folder node (e.g. "Encounters/Kael"),
        // built by walking up its (always built-in) ancestors. Used to anchor user triggers and
        // folders created inside the Built In library, since built-in folders have no stable ids.
        private static string GetBuiltInFolderPath(TreeTriggerFolder folder)
        {
            var parts = new System.Collections.Generic.List<string>();
            TreeViewItemBase current = folder;
            while (current is TreeTriggerFolder f && f.IsBuiltIn)
            {
                parts.Insert(0, f.Name);
                current = current.Parent;
            }
            return string.Join("/", parts);
        }

        private void AddTrigger(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase parent && (parent is TreeGlobal || parent is TreeTriggerFolder))
            {
                var parentFolder = parent as TreeTriggerFolder;
                var vm = new TriggerViewModel(settings, eQToolSettingsLoad, eqSpells)
                {
                    // A built-in parent has no usable id; the trigger anchors to it by path below.
                    FolderId = parentFolder?.IsBuiltIn == true ? null : parentFolder?.Backing.Id
                };
                if (parentFolder?.IsBuiltIn == true)
                {
                    vm.Model.BuiltInFolderPath = GetBuiltInFolderPath(parentFolder);
                }
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
                var parentFolder = parent as TreeTriggerFolder;
                var backing = new TriggerFolder
                {
                    Name = "New Folder",
                    // A built-in parent has no usable id; the folder anchors to it by path instead.
                    ParentId = parentFolder?.IsBuiltIn == true ? null : parentFolder?.Backing.Id,
                    BuiltInParentPath = parentFolder?.IsBuiltIn == true ? GetBuiltInFolderPath(parentFolder) : null
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
            // Pasting INTO built-in folders is allowed (the pasted user items anchor there by
            // path when persisted); built-in items themselves still can't be cut or deleted.

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
                return NewTriggerNode(new TriggerViewModel(clone, settings, eQToolSettingsLoad, eqSpells), parent);
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

        // JSON round-trip deep copy of a trigger with a fresh id. IsBuiltIn is JsonIgnore, so the
        // clone is never marked built-in. BuiltInId is cleared so a copy of a built-in becomes a
        // fully independent, editable user trigger (and is shown in the tree, not treated as the
        // built-in's enabled marker).
        private Models.Trigger CloneTrigger(Models.Trigger source)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var clone = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Trigger>(json);
            clone.TriggerId = Guid.NewGuid();
            clone.FolderId = null;
            clone.BuiltInFolderPath = null;
            clone.BuiltInId = null;
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

        // Enables or disables the clicked trigger (and the rest of the multi-selection, including
        // triggers inside any selected folders). The clicked trigger's current state decides the
        // direction, so a mixed selection becomes uniformly enabled/disabled. Built-in and user
        // triggers are handled the same way - both carry their own TriggerEnabled - and the change
        // is persisted.
        private void ToggleTriggerEnabled(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is TreeTrigger clicked))
            {
                return;
            }
            var enable = !clicked.Trigger.TriggerEnabled;

            var nodes = new System.Collections.Generic.List<TreeTrigger>();
            foreach (var node in ResolveSelection(clicked))
            {
                CollectTriggerNodes(node, nodes);
            }

            var changed = false;
            foreach (var tt in nodes)
            {
                if (tt.Trigger.TriggerEnabled != enable)
                {
                    tt.Trigger.TriggerEnabled = enable;
                    changed = true;
                }
            }

            if (changed)
            {
                eQToolSettingsLoad.Save(settings);
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

        private void WalkAndCollect(TreeViewItemBase node, Guid? parentId, System.Collections.Generic.List<TriggerFolder> folders, string builtInPath = null)
        {
            foreach (var child in node.Children)
            {
                if (child is TreeTriggerFolder f)
                {
                    // Built-in library folders are created in code and never persisted themselves,
                    // but they may contain user folders/triggers - descend with the accumulated
                    // "/"-separated path so those children anchor back to this spot on reload.
                    if (f.IsBuiltIn)
                    {
                        var path = string.IsNullOrEmpty(builtInPath) ? f.Name : builtInPath + "/" + f.Name;
                        WalkAndCollect(f, null, folders, path);
                        continue;
                    }
                    f.Backing.ParentId = parentId;
                    // Anchored to a built-in folder only when it has no user parent.
                    f.Backing.BuiltInParentPath = parentId == null ? builtInPath : null;
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
                    t.Trigger.Model.BuiltInFolderPath = parentId == null ? builtInPath : null;
                }
            }
        }

        private void PlayerDelete(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is TreePlayer clicked))
            {
                return;
            }

            var players = ResolveCharacterSelection(clicked);
            var message = players.Count == 1
                ? $"Are you sure that you want to delete the saved settings for {players[0].Name}? This only deletes Pigparse data!"
                : $"Are you sure that you want to delete the saved settings for the {players.Count} selected characters? This only deletes Pigparse data!";
            var title = players.Count == 1 ? $"Delete Pigparse data for {players[0].Name}" : $"Delete Pigparse data for {players.Count} characters";
            var result = System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Captured before removal: detaching a node can reset its IsSelected binding.
            var deletingShownCharacter = players.Any(p => p.IsSelected);
            foreach (var t in players)
            {
                _ = settings.Players.Remove(t.Player);
                _ = t.Parent?.Children.Remove(t);
            }
            eQToolSettingsLoad.Save(settings);

            // Don't leave the detail editor showing a character that no longer exists.
            if (deletingShownCharacter)
            {
                CharacterUserControl = null;
            }
        }

        // Determines which characters an operation should act on: the full multi-selection
        // when the clicked character is part of it, otherwise just the clicked character.
        private System.Collections.Generic.List<TreePlayer> ResolveCharacterSelection(TreePlayer clicked)
        {
            var selected = new System.Collections.Generic.List<TreeViewItemBase>();
            foreach (var server in _characterTreeItems)
            {
                CollectMultiSelected(server, selected);
            }
            var players = selected.OfType<TreePlayer>().ToList();
            if (players.Count > 0 && players.Contains(clicked))
            {
                return players;
            }
            return new System.Collections.Generic.List<TreePlayer> { clicked };
        }

        private ObservableCollection<TreeViewItemBase> _triggerTreeItems = new ObservableCollection<TreeViewItemBase>();
        // Root(s) of the Triggers tab tree (the Triggers branch).
        public ObservableCollection<TreeViewItemBase> TriggerTreeItems
        {
            get => _triggerTreeItems;
            set
            {
                if (value != _triggerTreeItems)
                {
                    _triggerTreeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<TreeViewItemBase> _characterTreeItems = new ObservableCollection<TreeViewItemBase>();
        // Root(s) of the Characters tab tree (one node per server).
        public ObservableCollection<TreeViewItemBase> CharacterTreeItems
        {
            get => _characterTreeItems;
            set
            {
                if (value != _characterTreeItems)
                {
                    _characterTreeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        private UserControl _triggerUserControl;
        // Detail editor shown on the right of the Triggers tab for the selected trigger.
        public UserControl TriggerUserControl
        {
            get => _triggerUserControl;
            set
            {
                _triggerUserControl = value;
                OnPropertyChanged();
            }
        }

        private UserControl _characterUserControl;
        // Detail editor shown on the right of the Characters tab for the selected server/character.
        public UserControl CharacterUserControl
        {
            get => _characterUserControl;
            set
            {
                _characterUserControl = value;
                OnPropertyChanged();
            }
        }

        // Triggers tab selection: only an actual trigger has an editor; folders and the
        // root clear the detail pane.
        public void TriggerTreeSelected(TreeViewItemBase p)
        {
            if (p is TreeTrigger)
            {
                TriggerUserControl = userComponentFactory.CreateComponent(p.Type, p);
            }
            else
            {
                TriggerUserControl = null;
            }
        }

        // Characters tab selection: servers and players show their detail editor; the
        // Zone(s) placeholder clears the detail pane.
        public void CharacterTreeSelected(TreeViewItemBase p)
        {
            if (p is TreePlayer || p is TreeServer)
            {
                CharacterUserControl = userComponentFactory.CreateComponent(p.Type, p);
            }
            else
            {
                CharacterUserControl = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
