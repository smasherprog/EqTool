using EQTool.Models;
using EQTool.Services;

using EQToolShared.APIModels.UIFileControllerModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private readonly UIFileSyncService uiFileSyncService;
        private readonly TreeGlobal triggersRoot;
        // Sync status is pulled once per settings-window session, not on every tab switch.
        private bool characterSyncStatusChecked;
        private readonly System.Collections.Generic.List<TreeViewItemBase> clipboardNodes = new System.Collections.Generic.List<TreeViewItemBase>();
        private bool clipboardIsCopy;
        public SettingsManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory, EQToolSettings settings, EQToolSettingsLoad eQToolSettingsLoad, EQSpells eqSpells, UIFileSyncService uiFileSyncService)
        {
            this.userComponentFactory = userComponentFactory;
            this.settings = settings;
            this.eQToolSettingsLoad = eQToolSettingsLoad;
            this.eqSpells = eqSpells;
            this.uiFileSyncService = uiFileSyncService;

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

        // Built-in triggers are read-only and placed into their declared "/"-separated folders;
        // user triggers go into the user's own folders by FolderId. Both carry TriggerEnabled.
        private void BuildTriggerTree()
        {
            triggersRoot.Children.Clear();

            // built-in folders have no stable ids, so they are created on demand keyed by path
            var builtInFolderCache = new System.Collections.Generic.Dictionary<string, TreeTriggerFolder>(StringComparer.OrdinalIgnoreCase);

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
                    parent = GetOrCreateBuiltInFolder(triggersRoot, trigger.BuiltInFolderPath, builtInFolderCache);
                }
                parent.Children.Add(NewTriggerNode(new TriggerViewModel(trigger, settings, eQToolSettingsLoad, eqSpells), parent));
            }

            SortRecursive(triggersRoot);
        }

        // Re-seeds the built-in library exactly as a brand-new user would get it: all built-ins
        // present, but only those in the top-level Encounters folder enabled. Discards every
        // user-created trigger/folder and all customizations to built-ins.
        public void ResetTriggersToDefault()
        {
            settings.Triggers = new System.Collections.Generic.List<Models.Trigger>();
            settings.TriggerFolders = new System.Collections.Generic.List<TriggerFolder>();
            _ = EQToolSettingsLoad.SyncBuiltInTriggers(settings);
            eQToolSettingsLoad.Save(settings);
            BuildTriggerTree();
        }

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

        private void InsertChild(TreeViewItemBase parent, TreeViewItemBase node)
        {
            parent.Children.Add(node);
            SortChildren(parent);
        }

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

        // re-sorts on rename so the tree does not go out of order while the user types
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

        // the root node is hidden, so this backs right-clicking empty space in the tree
        public ContextMenu GetTriggerRootContextMenu()
        {
            return GetContextMenu(triggersRoot);
        }

        public ContextMenu GetContextMenu(TreeViewItemBase item)
        {
            if (item is TreeServer)
            {
                var menu = new ContextMenu();
                _ = menu.Items.Add(BuildMenuItem("Refresh UI Sync (all characters)", RefreshServerUi, item));
                return menu;
            }
            else if (item is TreeGlobal)
            {
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
                _ = menu.Items.Add(BuildMenuItem("Refresh UI Sync", RefreshCharacterUi, item));
                _ = menu.Items.Add(BuildMenuItem("Delete UI Data from Server", DeleteCharacterUi, item));
                _ = menu.Items.Add(new Separator());
                _ = menu.Items.Add(BuildMenuItem("Delete Saved Data", PlayerDelete, item));
                return menu;
            }

            return null;
        }

        private void ExpandAll(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase node)
            {
                SetExpandedRecursive(node, true);
            }
        }

        private void CollapseAll(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreeViewItemBase node)
            {
                SetExpandedRecursive(node, false);
            }
        }

        // triggersRoot is never a visible TreeViewItem, so it is skipped rather than expanded
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

        // Built-in folders have no stable ids, so items created inside the library anchor to
        // this "/"-separated path (e.g. "Encounters/Kael") instead.
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
                    // a built-in parent has no id, so it anchors by path instead
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
                    // a built-in parent has no id, so it anchors by path instead
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

        // Acts on the whole multi-selection when the clicked item is part of it, else just the
        // clicked item. Nested selections collapse to their top-most nodes to avoid double work.
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
            // no built-in guard here on purpose: pasting INTO a built-in folder is allowed, since
            // the pasted user items anchor there by path when persisted

            if (clipboardIsCopy)
            {
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

            // skip nodes that would be pasted into themselves or their own subtree
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

        // IsBuiltIn is JsonIgnore, so the round-trip alone drops built-in status; clearing
        // BuiltInId makes a copy of a built-in a fully independent user trigger rather than
        // the built-in's enabled marker.
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

        // The clicked trigger's state decides the direction for the whole selection, so a mixed
        // selection ends up uniformly enabled or disabled rather than each item flipping.
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

        // settings stores flat lists, so the whole tree is re-flattened after any structural change
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
                    // Built-in folders are never persisted themselves, but may contain user items -
                    // descend with the accumulated path so those children anchor back here on reload.
                    if (f.IsBuiltIn)
                    {
                        var path = string.IsNullOrEmpty(builtInPath) ? f.Name : builtInPath + "/" + f.Name;
                        WalkAndCollect(f, null, folders, path);
                        continue;
                    }
                    f.Backing.ParentId = parentId;
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

        public void RefreshAllCharacterSyncStatusOnce()
        {
            if (characterSyncStatusChecked)
            {
                return;
            }
            RefreshAllCharacterSyncStatus();
        }

        public void RefreshAllCharacterSyncStatus()
        {
            var players = new List<TreePlayer>();
            foreach (var node in _characterTreeItems)
            {
                CollectTreePlayers(node, players);
            }
            characterSyncStatusChecked = true;
            RefreshStatuses(players);
        }

        public void RefreshCharacterSyncStatus(TreePlayer player)
        {
            if (player != null)
            {
                RefreshStatuses(new List<TreePlayer> { player });
            }
        }

        private void RefreshStatuses(List<TreePlayer> players)
        {
            if (players == null || players.Count == 0)
            {
                return;
            }
            _ = Task.Factory.StartNew(() =>
            {
                var serverFiles = uiFileSyncService.GetServerFiles();
                var localKeys = new HashSet<string>(
                    uiFileSyncService.GetLocalUiFiles().Select(i => CharacterKey(i.PlayerName, i.Server)));
                _ = Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var tp in players)
                    {
                        ApplyStatus(tp, serverFiles, localKeys);
                    }
                }));
            });
        }

        // Green check = backed up on the server, red circle-X = a local UI file exists but is
        // not backed up, nothing = no UI file for that character.
        private static void ApplyStatus(TreePlayer tp, List<UIFileMetadata> serverFiles, HashSet<string> localKeys)
        {
            var name = tp.Player?.Name;
            var server = tp.Player?.Server;
            if (string.IsNullOrWhiteSpace(name) || server == null)
            {
                tp.UiSyncStatus = UiSyncStatus.None;
                tp.UiSyncDate = string.Empty;
                return;
            }

            var matches = serverFiles
                .Where(f => f.Server == server.Value && string.Equals(f.PlayerName, name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count > 0)
            {
                tp.UiSyncStatus = UiSyncStatus.Synced;
                tp.UiSyncDate = matches.Max(f => f.LastModifiedUtc).ToString("yyyy-MM-dd hh:mm tt");
            }
            else if (localKeys.Contains(CharacterKey(name, server.Value)))
            {
                tp.UiSyncStatus = UiSyncStatus.NotSynced;
                tp.UiSyncDate = string.Empty;
            }
            else
            {
                tp.UiSyncStatus = UiSyncStatus.None;
                tp.UiSyncDate = string.Empty;
            }
        }

        private static string CharacterKey(string name, Servers server)
        {
            return (name ?? string.Empty).ToLowerInvariant() + "|" + server;
        }

        private void CollectTreePlayers(TreeViewItemBase node, List<TreePlayer> acc)
        {
            if (node is TreePlayer tp)
            {
                acc.Add(tp);
            }
            foreach (var child in node.Children)
            {
                CollectTreePlayers(child, acc);
            }
        }

        private void RefreshServerUi(object sender, RoutedEventArgs e)
        {
            RefreshAllCharacterSyncStatus();
        }

        private void RefreshCharacterUi(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.Tag is TreePlayer clicked)
            {
                RefreshCharacterSyncStatus(clicked);
            }
        }

        private void DeleteCharacterUi(object sender, RoutedEventArgs e)
        {
            if (!((sender as MenuItem)?.Tag is TreePlayer clicked))
            {
                return;
            }
            var name = clicked.Player?.Name;
            var server = clicked.Player?.Server;
            if (string.IsNullOrWhiteSpace(name) || server == null)
            {
                return;
            }
            var result = MessageBox.Show(
                $"Delete the server UI backup for {name} ({server})? Your local UI files are not affected.",
                "Delete UI backup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            _ = Task.Factory.StartNew(() =>
            {
                var files = uiFileSyncService.GetServerFiles()
                    .Where(f => f.Server == server.Value && string.Equals(f.PlayerName, name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var f in files)
                {
                    _ = uiFileSyncService.DeleteServerFile(f.FileName);
                }
                RefreshCharacterSyncStatus(clicked);
            });
        }

        private ObservableCollection<TreeViewItemBase> _triggerTreeItems = new ObservableCollection<TreeViewItemBase>();
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
        public UserControl CharacterUserControl
        {
            get => _characterUserControl;
            set
            {
                _characterUserControl = value;
                OnPropertyChanged();
            }
        }

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
