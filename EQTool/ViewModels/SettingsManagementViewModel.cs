using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EQTool.ViewModels
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger()
        {
        }

        public string Name { get; set; }
    }

    public class TreePlayer : TreeViewItemBase
    {
        public TreePlayer()
        {
            Children = new ObservableCollection<TreeTrigger>();
        }

        public string Name { get; set; }

        public ObservableCollection<TreeTrigger> Children { get; set; }
    }

    public class TreeServer : TreeViewItemBase
    {
        public TreeServer()
        {
            Children = new ObservableCollection<TreePlayer>();
        }

        public string Name { get; set; }

        public Servers Servers { get; set; }

        public ObservableCollection<TreePlayer> Children { get; set; }
    }

    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SettingsManagementViewModel : INotifyPropertyChanged
    {
        private readonly UserComponentSettingsManagementFactory userComponentFactory;
        private readonly EQToolSettings settings;

        public SettingsManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory, EQToolSettings settings)
        {
            this.userComponentFactory = userComponentFactory;
            this.settings = settings;
            foreach (var item in Enum.GetValues(typeof(Servers)).Cast<Servers>().Where(a => a != Servers.MaxServers && a != Servers.Quarm).ToList())
            {
                var players = settings.Players.Where(a => a.Server == item).ToList();
                var serv = new TreeServer
                {
                    Children = new ObservableCollection<TreePlayer>(),
                    Name = item.ToString()
                };
                _TreeItems.Add(serv);
                serv.Children.Add(new TreePlayer
                {
                    Name = "Global",
                    Children = new ObservableCollection<TreeTrigger>()
                });
                serv.Children.Add(new TreePlayer
                {
                    Name = "Zone(s)",
                    Children = new ObservableCollection<TreeTrigger>()
                });
                foreach (var p in players)
                {
                    serv.Children.Add(new TreePlayer
                    {
                        Name = p.Name,
                        Children = new ObservableCollection<TreeTrigger>()
                    });
                }
            }
            UserControl = this.userComponentFactory.CreateComponent(UserComponentSettingsManagementTypes.Landing);
        }

        public ContextMenu GetContextMenu(TreeViewItemBase item)
        {
            if (item is TreeServer)
            {
                var mnuItem1 = new MenuItem
                {
                    Header = "New Package"
                };

                var mnuItem2 = new MenuItem
                {
                    Header = "Show Package Details"
                };
                var mnuItem3 = new MenuItem
                {
                    Header = "Edit Package"
                };
                var mnuItem4 = new MenuItem
                {
                    Header = "Delete Package"
                };
                var mnuItem5 = new MenuItem
                {
                    Header = "Add to Queue"
                };

                var menu = new ContextMenu() { };
                _ = menu.Items.Add(mnuItem1);
                _ = menu.Items.Add(mnuItem2);
                _ = menu.Items.Add(mnuItem3);
                _ = menu.Items.Add(mnuItem4);
                _ = menu.Items.Add(mnuItem5);
                foreach (MenuItem i in menu.Items)
                {
                    i.Tag = item;
                    i.Click += ServerMenuNewPackageClicked;
                }
                return menu;
            }
            else if (item is TreePlayer)
            {
                var mnuItem1 = new MenuItem
                {
                    Header = "Show Batch Details"
                };
                var mnuItem2 = new MenuItem
                {
                    Header = "Edit Batch"
                };
                var mnuItem3 = new MenuItem
                {
                    Header = "Delete Batch"
                };

                var menu = new ContextMenu() { };
                _ = menu.Items.Add(mnuItem1);
                _ = menu.Items.Add(mnuItem2);
                _ = menu.Items.Add(mnuItem3);
                foreach (MenuItem i in menu.Items)
                {
                    i.Tag = item;
                    i.Click += PlayerMenuNewPackageClicked;
                }
                return menu;
            }

            return new ContextMenu();
        }

        private void ServerMenuNewPackageClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var s = sender as MenuItem;
            if (s.Tag is TreeServer t)
            {
                Debug.WriteLine($"Server {t.Name} Selected with {s.Header}");
            }
        }
        private void PlayerMenuNewPackageClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var s = sender as MenuItem;
            if (s.Tag is TreePlayer t)
            {
                Debug.WriteLine($"Player {t.Name} Selected with {s.Header}");
            }
        }

        private ObservableCollection<TreeServer> _TreeItems = new ObservableCollection<TreeServer>();
        public ObservableCollection<TreeServer> TreeItems
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
            if (p is TreeServer)
            {
                UserControl = userComponentFactory.CreateComponent(UserComponentSettingsManagementTypes.Server, p);
            }
            else if (p is TreePlayer)
            {
                UserControl = userComponentFactory.CreateComponent(UserComponentSettingsManagementTypes.Player, p);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
