using System;
using System.Diagnostics;
using System.Windows;
using EQTool.Services;
using EQToolShared;

namespace EQTool.ViewModels.SpellWindow
{
    [DebuggerDisplay("Group = '{DisplayGroup}' Sorting = '{GroupSorting}' | Id = '{Id}', Target = '{Target}'")]
    public class BoatViewModel : TimerViewModel
    {
        public BoatViewModel()
        {
            Target = "Boat Schedules";
            DeleteButtonVisibility = Visibility.Collapsed;
        }

        public string LastSeen { get; set; }
        private DateTimeOffset _LastSeenDateTime { get; set; }
        public DateTimeOffset LastSeenDateTime
        {
            get => _LastSeenDateTime;
            set
            {
                _LastSeenDateTime = value;
                LastSeen = value.LocalDateTime.ToString("MM/dd hh:mm:ss tt");
                OnPropertyChanged(nameof(LastSeen));
            }
        }
        public BoatInfo Boat { get; set; }
        public override string GroupSorting => SortingPrefixes.Bottommost + DisplayGroup;
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Boat;
    }
}
