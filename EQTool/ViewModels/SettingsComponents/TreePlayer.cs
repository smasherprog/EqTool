using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreePlayer : TreeViewItemBase
    {
        public TreePlayer()
        {
            Children = new ObservableCollection<TreeTrigger>();
        }

        public string Name { get; set; }

        public ObservableCollection<TreeTrigger> Children { get; set; }
    }
}
