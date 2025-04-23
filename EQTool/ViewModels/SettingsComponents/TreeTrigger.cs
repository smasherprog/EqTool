using EQTool.Models;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger(Trigger userTrigger)
        {
            this.Trigger = userTrigger;
        }

        public Trigger Trigger { get; set; }

        public string Name { get { return Trigger.TriggerName; } }
        public ObservableCollection<TreeViewItemBase> Children { get; set; } = new ObservableCollection<TreeViewItemBase>();
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
