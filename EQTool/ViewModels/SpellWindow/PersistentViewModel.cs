using EQTool.Models;
using EQToolShared.Enums;
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
        Timer,
        Boat
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

        private Visibility _DeleteButtonVisibility = Visibility.Visible;

        public Visibility DeleteButtonVisibility
        {
            get => _DeleteButtonVisibility;
            set
            {
                if (_DeleteButtonVisibility == value)
                {
                    return;
                }
                _DeleteButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public virtual SpellViewModelType SpellViewModelType => SpellViewModelType.Persistent;

        private Visibility _ColumnVisibility = Visibility.Visible;

        public Visibility ColumnVisibility
        {
            get => _ColumnVisibility;
            set
            {
                if (_ColumnVisibility == value)
                {
                    return;
                }
                _ColumnVisibility = value;
                OnPropertyChanged();
            }
        }

        public virtual string Sorting => _GroupName;
        private string _GroupName = string.Empty;
        public string GroupName
        {
            get => _GroupName;
            set
            {
                if (_GroupName == value)
                {
                    return;
                }
                _GroupName = value;
                OnPropertyChanged();
            }
        }

        public string TargetClassString { get; set; }
        private PlayerClasses? _TargetClass;

        public PlayerClasses? TargetClass
        {
            get => _TargetClass;
            set
            {
                _TargetClass = value;
                TargetClassString = _TargetClass.HasValue ? _TargetClass.Value.ToString() : string.Empty;
                OnPropertyChanged(nameof(TargetClassString));
                OnPropertyChanged();
            }
        }

        public SolidColorBrush ProgressBarColor { get; set; } = Brushes.DarkSeaGreen;
        public DateTime UpdatedDateTime { get; set; } = DateTime.Now;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
