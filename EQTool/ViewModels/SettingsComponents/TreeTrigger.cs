using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger(TriggerViewModel userTrigger, TreeViewItemBase parent) : base(parent)
        {
            this.Trigger = userTrigger;
        }

        public TriggerViewModel Trigger { get; set; }
        public override string Name { get { return Trigger.TriggerName; } }
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
