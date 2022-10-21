using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.Models
{
    public class UISpell : INotifyPropertyChanged
    {
        public SpellIcon SpellIcon { get; set; }
        public bool HasSpellIcon => SpellIcon != null;
        public Int32Rect Rect { get; set; }
        public string SpellName { get; set; }
        public string TargetName { get; set; }
        private int _SecondsLeftOnSpell = 0;
        public int SecondsLeftOnSpell
        {
            get => _SecondsLeftOnSpell;
            set
            {
                _SecondsLeftOnSpell = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
