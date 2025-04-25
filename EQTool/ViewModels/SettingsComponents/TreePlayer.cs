using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreePlayer : TreeViewItemBase
    {
        public TreePlayer(TreeViewItemBase parent) : base(parent)
        {
        }

        public override string Name => Player.Name + (!string.IsNullOrWhiteSpace(Player.GuildName) ? $"<{Player.GuildName}>" : string.Empty);
        public string LastPlayed => Player.LastUpdate.HasValue ? Player.LastUpdate.Value.ToShortDateString() : string.Empty;
        public PlayerInfo Player { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Player;
    }
}
