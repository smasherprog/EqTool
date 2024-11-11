namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeGeneral : TreeViewItemBase
    {
        public TreeGeneral()
        {
        }

        public string Name { get; set; }

        public override TreeViewItemType Type => TreeViewItemType.General;
    }
}
