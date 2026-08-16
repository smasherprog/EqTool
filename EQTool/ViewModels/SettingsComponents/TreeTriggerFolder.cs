using EQTool.Models;

namespace EQTool.ViewModels.SettingsComponents
{
    public class TreeTriggerFolder : TreeViewItemBase
    {
        public TreeTriggerFolder(TriggerFolder backing, TreeViewItemBase parent) : base(parent)
        {
            this.Backing = backing;
        }

        public TriggerFolder Backing { get; }

        public bool IsBuiltIn { get; set; }

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

        public void BeginEdit()
        {
            editBackup = Backing.Name;
            IsEditing = true;
        }

        public void CancelEdit()
        {
            EditableName = editBackup;
            IsEditing = false;
        }
    }
}
