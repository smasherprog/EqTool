using EQToolShared.Enums;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeGlobal : TreeViewItemBase
    {
        public TreeGlobal(string name, TreeViewItemBase parent) : base(parent)
        {
            this._Name = name;
        }
        private string _Name = string.Empty;
        public override string Name
        {
            get { return this._Name; }
        }

        public Servers Server { get; set; }

        public override TreeViewItemType Type => TreeViewItemType.Global;
    }

}
