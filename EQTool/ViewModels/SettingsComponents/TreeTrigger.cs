using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger(TriggerViewModel userTrigger, TreeViewItemBase parent) : base(parent)
        {
            this.Trigger = userTrigger;
            this.Trigger.PropertyChanged += Trigger_PropertyChanged;
        }

        private void Trigger_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Trigger.TriggerName))
            {
                this.OnPropertyChanged(nameof(Name));
            }
            else if (e.PropertyName == nameof(Trigger.TriggerEnabled))
            {
                this.OnPropertyChanged(nameof(IsTriggerEnabled));
            }
        }

        public TriggerViewModel Trigger { get; set; }
        public override string Name { get { return Trigger.TriggerName; } }
        public bool IsBuiltIn => Trigger?.IsBuiltIn ?? false;
        // Whether this trigger is enabled; the tree grays out the name of disabled triggers.
        public bool IsTriggerEnabled => Trigger?.TriggerEnabled ?? false;
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
