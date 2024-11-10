using EQToolShared.Enums;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeServer : TreeViewItemBase
    {
        public TreeServer()
        {
            Children = new ObservableCollection<TreePlayer>();
        }

        public string Name { get; set; }

        public Servers Server { get; set; }

        public ObservableCollection<TreePlayer> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Server;
    }

}
