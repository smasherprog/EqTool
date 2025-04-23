using EQTool.Models;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public Trigger Trigger;

        public TreeTrigger(Trigger userTrigger)
        {
            this.Trigger = userTrigger;
        }

        public string Name { get; set; }
        public ObservableCollection<TreeViewItemBase> Children { get; set; } = new ObservableCollection<TreeViewItemBase>();
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
