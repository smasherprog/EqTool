using EQToolShared.Enums;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeGlobal : TreeViewItemBase
    {
        public TreeGlobal()
        {
            Children = new ObservableCollection<TreeTrigger>();
        }

        public string Name { get; set; }

        public Servers Server { get; set; }

        public ObservableCollection<TreeTrigger> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Global;
    }

}
