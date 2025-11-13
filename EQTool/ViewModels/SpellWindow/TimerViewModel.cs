using System;
using EQTool.Models;
using EQTool.Services;

namespace EQTool.ViewModels.SpellWindow
{
    public class TimerViewModel : PersistentViewModel
    {
        public TimeSpan TotalDuration { get; set; }

        private TimeSpan _TotalRemainingDuration = TimeSpan.Zero;
        public TimeSpan TotalRemainingDuration
        {
            get => _TotalRemainingDuration;
            set
            {
                _TotalRemainingDuration = value;
                if (_TotalRemainingDuration.TotalSeconds > 0)
                {
                    PercentLeft = (int)(_TotalRemainingDuration.TotalSeconds / TotalDuration.TotalSeconds * 100);
                }

                OnPropertyChanged(nameof(SecondsLeftPretty));
                OnPropertyChanged(nameof(PercentLeft));
            }
        }
        public int PercentLeft { get; set; } = 100;

        public string SecondsLeftPretty
        {
            get
            {
                var st = "";
                if (_TotalRemainingDuration.Hours > 0)
                {
                    st += _TotalRemainingDuration.Hours + "h ";
                }
                if (_TotalRemainingDuration.Minutes > 0)
                {
                    st += _TotalRemainingDuration.Minutes + "m ";
                }
                if (_TotalRemainingDuration.Seconds > 0)
                {
                    st += _TotalRemainingDuration.Seconds + "s";
                }
                return st;
            }
        }
        
        public override string DisplayGroup => IsCategorizeById ? Id.TrimEnd(" Cooldown") : Target; // For cooldowns, we want it to be grouped with the spell itself.
        public override string GroupSorting
        {
            get
            {
                if (DisplayGroup == CustomTimer.CustomerTime)
                {
                    return SortingPrefixes.Topmost + DisplayGroup;
                }
                return base.GroupSorting;
            }
        }
        
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Timer;
    }
}
