namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeGeneral : TreeViewItemBase
    {
        public TreeGeneral(string name, TreeViewItemBase parent) : base(parent)
        {
            this._Name = name;
        }
        private string _Name = string.Empty;
        public override string Name { get { return this._Name; } }

        public override TreeViewItemType Type => TreeViewItemType.General;
    }
}
