using EQToolShared.Enums;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeServer : TreeViewItemBase
    {
        public TreeServer(string name, TreeViewItemBase parent) : base(parent)
        {
            this._Name = name;
        }

        private string _Name = string.Empty;
        public override string Name { get { return _Name; } }

        public Servers Server { get; set; }

        public override TreeViewItemType Type => TreeViewItemType.Server;
    }

}
