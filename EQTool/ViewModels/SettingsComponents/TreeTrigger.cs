using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger()
        {
            Children = new ObservableCollection<TreeViewItemBase>();
        }

        public string Name { get; set; }
        public ObservableCollection<TreeViewItemBase> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
