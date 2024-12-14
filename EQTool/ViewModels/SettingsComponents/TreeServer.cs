using EQToolShared.Enums;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeServer : TreeViewItemBase
    {
        public TreeServer()
        {
            Children = new ObservableCollection<TreeViewItemBase>();
        }

        public string Name { get; set; }

        public Servers Server { get; set; }

        public ObservableCollection<TreeViewItemBase> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Server;
    }

}
