using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using EQTool.Services;

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

    [DebuggerDisplay("Group = '{DisplayGroup}' Sorting = '{GroupSorting}' | Id = '{Id}', Target = '{Target}'")]
    public abstract class PersistentViewModel : INotifyPropertyChanged
    {
        public SpellIcon Icon { get; set; }
        public bool HasIcon => Icon != null;
        public Int32Rect Rect { get; set; }
        
        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        
        private string _target;
        public string Target
        {
            get => _target;
            set
            {
                _target = value;
                IsPlayerTarget = !_target.StartsWith(" ") || _target == EQSpells.SpaceYou;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPlayerTarget));
                OnPropertyChanged(nameof(DisplayGroup));
                OnPropertyChanged(nameof(GroupSorting));
            }
        }

        public bool IsPlayerTarget { get; private set; }

        public virtual string DisplayName => IsCategorizeById ? Target : Id;
        public virtual string DisplayGroup => IsCategorizeById ? Id : Target;
        public virtual string GroupSorting
        {
            get
            {
                var groupName = DisplayGroup;
                if (groupName.StartsWith(" ") && groupName != EQSpells.SpaceYou)
                {
                    return SortingPrefixes.Primary + groupName;
                }
                if (groupName == EQSpells.SpaceYou)
                {
                    return SortingPrefixes.Secondary + groupName;
                }
                return SortingPrefixes.Tertiary + groupName;
            }
        }
        
        private bool _IsCategorizeById;
        public bool IsCategorizeById
        {
            get => _IsCategorizeById;
            set
            {
                if (_IsCategorizeById == value)
                {
                    return;
                }
                _IsCategorizeById = value;
                UpdateTargetClassString();
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayGroup));
                OnPropertyChanged(nameof(GroupSorting));
            }
        }
        
        protected Visibility _HeaderVisibility = Visibility.Visible;
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

        protected Visibility _DeleteButtonVisibility = Visibility.Visible;
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

        protected Visibility _ColumnVisibility = Visibility.Visible;
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
        
        public virtual bool ShouldKeepTargetClassString => !IsCategorizeById;
        
        private string _targetClassString;
        public string TargetClassString
        {
            get => _targetClassString;
            set
            {
                _targetClassString = value;
                OnPropertyChanged();
            }
        }
        
        protected PlayerClasses? _TargetClass;
        public PlayerClasses? TargetClass
        {
            get => _TargetClass;
            set
            {
                _TargetClass = value;
                UpdateTargetClassString();
                OnPropertyChanged();
            }
        }

        protected void UpdateTargetClassString()
        {
            if (!ShouldKeepTargetClassString)
                TargetClassString = string.Empty;
            else
                TargetClassString = _TargetClass.HasValue ? _TargetClass.Value.ToString() : string.Empty;
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
