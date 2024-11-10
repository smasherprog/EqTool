namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTrigger : TreeViewItemBase
    {
        public TreeTrigger()
        {
        }

        public string Name { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Trigger;
    }

}
