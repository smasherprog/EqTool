using EQTool.Models;
using System.Collections.ObjectModel;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreePlayer : TreeViewItemBase
    {
        public TreePlayer()
        {
            Children = new ObservableCollection<TreeTrigger>();
        }

        public TreeServer Parent { get; set; }
        public string Name => Player.Name + (!string.IsNullOrWhiteSpace(Player.GuildName) ? $"<{Player.GuildName}>" : string.Empty);
        public string LastPlayed => Player.LastUpdate.HasValue ? Player.LastUpdate.Value.ToShortDateString() : string.Empty;
        public PlayerInfo Player { get; set; }
        public ObservableCollection<TreeTrigger> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Player;
    }
}
