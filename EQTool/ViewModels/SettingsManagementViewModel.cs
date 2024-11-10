using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EQTool.ViewModels
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger()
        {
            Children = new ObservableCollection<string>();
        }

        public string Name { get; set; }

        public ObservableCollection<string> Children { get; set; }// THIS IS A PLACEHOLDER FOR NOW
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
