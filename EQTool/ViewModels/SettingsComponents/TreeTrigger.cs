using EQTool.Models;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger(TriggerViewModel userTrigger)
        {
            this.Trigger = userTrigger;
        }

        public TriggerViewModel Trigger { get; set; }
        public string Name { get { return Trigger.TriggerName; } }
        public ObservableCollection<TreeViewItemBase> Children { get; set; } = new ObservableCollection<TreeViewItemBase>();
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
