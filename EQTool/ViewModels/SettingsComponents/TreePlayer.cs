using EQTool.Models;
using System.Collections.ObjectModel;
using System.Windows;

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
        private readonly bool _IsActive = false;
        public bool IsActive
        {
            get => _IsActive;
            set
            {
                if (_IsActive != value)
                {
                    if (_IsActive)
                    {
                        NameWeight = FontWeights.Bold;
                    }
                    else
                    {
                        NameWeight = FontWeights.Normal;
                    }
                }

            }
        }
        public FontWeight NameWeight { get; set; } = FontWeights.Normal;
        public PlayerInfo Player { get; set; }
        public ObservableCollection<TreeTrigger> Children { get; set; }
        public override TreeViewItemType Type => TreeViewItemType.Player;
    }
}
