using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;

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
            _TreeItems.Add(new TreeGeneral
            {
                Name = "General",
                IsSelected = true
            });

            var triggers = new TreeGlobal
            {
                Name = "Triggers",
                Children = new ObservableCollection<TreeTrigger>()
            };
            _TreeItems.Add(triggers);
            if (!settings.Triggers.Any())
            {
                foreach (var trigger in UserDefinedTriggerHandler.TriggerList)
                {
                    settings.Triggers.Add(new Models.Trigger
                    {
                        TriggerEnabled = trigger.TriggerEnabled,
                        TriggerName = trigger.TriggerName,
                        TriggerRegExString = trigger.SearchText
                    });
                }
            }
            eQToolSettingsLoad.Save(settings);

            foreach (var trigger in settings.Triggers)
            {
                triggers.Children.Add(new TreeTrigger(trigger));
            }

            foreach (var item in Enum.GetValues(typeof(Servers)).Cast<Servers>().Where(a => a != Servers.MaxServers && a != Servers.Quarm).ToList())
            {
                var players = settings.Players.Where(a => a.Server == item).ToList();
                var treeServer = new TreeServer
                {
                    Children = new ObservableCollection<TreeViewItemBase>(),
                    Name = item.ToString()
                };
                _TreeItems.Add(treeServer);
                treeServer.Children.Add(new TreeZone
                {
                    Name = "Zone(s)",
                    Children = new ObservableCollection<TreeTrigger>()
                });
                foreach (var p in players.OrderBy(a => a.Name))
                {
                    treeServer.Children.Add(new TreePlayer
                    {
                        Player = p,
                        Parent = treeServer,
                        Children = new ObservableCollection<TreeTrigger>()
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
#if !RELEASE
            UserControl = userComponentFactory.CreateComponent(p.Type, p);
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
