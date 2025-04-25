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
        public SettingsManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory, EQToolSettings settings, EQToolSettingsLoad eQToolSettingsLoad)
        {
            this.userComponentFactory = userComponentFactory;
            this.settings = settings;
            this.eQToolSettingsLoad = eQToolSettingsLoad;
            _TreeItems.Add(new TreeGeneral("General", null)
            {
                IsSelected = true
            });

            var triggers = new TreeGlobal("Triggers", null);
            _TreeItems.Add(triggers);

            foreach (var trigger in settings.Triggers)
            {
                triggers.Children.Add(new TreeTrigger(new TriggerViewModel(trigger, settings, eQToolSettingsLoad), triggers));
            }

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

        public ContextMenu GetContextMenu(TreeViewItemBase item)
        {
            if (item is TreeServer)
            {
                //no menu yet
                return null;
            }
            else if (item is TreeGlobal)
            {
                var mnuItem3 = new MenuItem
                {
                    Header = "Add Trigger"
                };
                mnuItem3.Click += AddTrigger;
                var menu = new ContextMenu() { };
                _ = menu.Items.Add(mnuItem3);
                foreach (MenuItem i in menu.Items)
                {
                    i.Tag = item;
                }
                return menu;
            }
            else if (item is TreeTrigger)
            {
                var mnuItem3 = new MenuItem
                {
                    Header = "Delete Trigger"
                };
                mnuItem3.Click += DeleteTrigger;
                var menu = new ContextMenu() { };
                _ = menu.Items.Add(mnuItem3);
                foreach (MenuItem i in menu.Items)
                {
                    i.Tag = item;
                }
                return menu;
            }
            else if (item is TreePlayer)
            {
                var mnuItem3 = new MenuItem
                {
                    Header = "Delete Saved Data"
                };
                mnuItem3.Click += PlayerDelete;
                var menu = new ContextMenu() { };
                _ = menu.Items.Add(mnuItem3);
                foreach (MenuItem i in menu.Items)
                {
                    i.Tag = item;
                }
                return menu;
            }

            return null;
        }

        private void AddTrigger(object sender, RoutedEventArgs e)
        {
            var s = sender as MenuItem;
            if (s.Tag is TreeGlobal t)
            {
                var newtrigger = new TreeTrigger(new TriggerViewModel(settings, eQToolSettingsLoad), t);
                newtrigger.IsSelected = true;
                t.Children.Insert(0, newtrigger);
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
                    eQToolSettingsLoad.Save(settings);
                    _ = t.Parent.Children.Remove(t);
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
