using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    // A nestable folder within the Triggers branch. Folders can contain other
    // folders and triggers, can be named/renamed inline, deleted, cut and pasted.
    public class TreeTriggerFolder : TreeViewItemBase
    {
        public TreeTriggerFolder(TriggerFolder backing, TreeViewItemBase parent) : base(parent)
        {
            this.Backing = backing;
        }

        public TriggerFolder Backing { get; }

        public override string Name => Backing.Name;

        // Bound TwoWay by the inline rename TextBox. Kept separate from the
        // abstract get-only Name on the base class.
        public string EditableName
        {
            get => Backing.Name;
            set
            {
                if (Backing.Name != value)
                {
                    Backing.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public override TreeViewItemType Type => TreeViewItemType.TriggerFolder;

        private string editBackup;

        // Start inline editing, remembering the current name so the edit can be cancelled.
        public void BeginEdit()
        {
            editBackup = Backing.Name;
            IsEditing = true;
        }

        // Cancel inline editing and restore the name as it was before editing started.
        public void CancelEdit()
        {
            EditableName = editBackup;
            IsEditing = false;
        }
    }
}
