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
        }

        public TriggerViewModel Trigger { get; set; }
        public override string Name { get { return Trigger.TriggerName; } }
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
