using EQTool.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
    public enum SpellViewModelType
    {
        Persistent,
        Counter,
        Roll,
        Spell,
        Timer
    }

    public class PersistentViewModel : INotifyPropertyChanged
    {
        public SpellIcon Icon { get; set; }
        public bool HasIcon => Icon != null;
        public Int32Rect Rect { get; set; }

        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private Visibility _HeaderVisibility = Visibility.Visible;

        public Visibility HeaderVisibility
        {
            get => _HeaderVisibility;
            set
            {
                if (_HeaderVisibility == value)
                {
                    return;
                }
                _HeaderVisibility = value;
                OnPropertyChanged();
            }
        }

        public virtual SpellViewModelType SpellViewModelType => SpellViewModelType.Persistent;
        public virtual Visibility ColumnVisibility => Visibility.Visible;

        public virtual string Sorting => GroupName;
        public string GroupName { get; set; }
        public SolidColorBrush ProgressBarColor { get; set; } = Brushes.DarkSeaGreen;
        public DateTime UpdatedDateTime { get; set; } = DateTime.Now;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
