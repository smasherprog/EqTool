using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    public enum UiSyncStatus
    {
        None,       // no local UI file and no server backup - show nothing
        NotSynced,  // a UI file exists but is not backed up on the server
        Synced      // backed up on the server
    }

    public class TreePlayer : TreeViewItemBase
    {
        public TreePlayer(TreeViewItemBase parent) : base(parent)
        {
        }

        public override string Name => Player.Name + (!string.IsNullOrWhiteSpace(Player.GuildName) ? $"<{Player.GuildName}>" : string.Empty);
        public string LastPlayed => Player.LastUpdate.HasValue ? Player.LastUpdate.Value.ToShortDateString() : string.Empty;
        public PlayerInfo Player { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Player;

        private UiSyncStatus _uiSyncStatus = UiSyncStatus.None;
        // UI-file backup status for this character, shown as an icon next to the name.
        public UiSyncStatus UiSyncStatus
        {
            get => _uiSyncStatus;
            set { _uiSyncStatus = value; OnPropertyChanged(); }
        }

        private string _uiSyncDate = string.Empty;
        // Server backup date (empty unless Synced).
        public string UiSyncDate
        {
            get => _uiSyncDate;
            set { _uiSyncDate = value; OnPropertyChanged(); }
        }
    }
}
