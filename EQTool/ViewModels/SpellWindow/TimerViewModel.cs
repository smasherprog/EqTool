using System;

namespace EQTool.ViewModels.SpellWindow
{
    public class TimerViewModel : BaseTriggerViewModel
    {
        public TimeSpan TotalDuration { get; set; }

        // how long the timer stays in the list after reaching zero, counting negative, before it is removed
        public TimeSpan ExpiredLingerDuration { get; set; } = TimeSpan.Zero;

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
                else
                {
                    PercentLeft = 0;
                }

                OnPropertyChanged(nameof(SecondsLeftPretty));
                OnPropertyChanged(nameof(PercentLeft));
                OnPropertyChanged(nameof(IsExpired));
            }
        }
        public int PercentLeft { get; set; } = 100;

        public bool IsExpired => _TotalRemainingDuration.TotalSeconds <= 0;

        public string SecondsLeftPretty
        {
            get
            {
                var remaining = _TotalRemainingDuration.Duration();
                var st = "";
                if (remaining.Hours > 0)
                {
                    st += remaining.Hours + "h ";
                }
                if (remaining.Minutes > 0)
                {
                    st += remaining.Minutes + "m ";
                }
                if (remaining.Seconds > 0)
                {
                    st += remaining.Seconds + "s";
                }
                if (IsExpired && st.Length > 0)
                {
                    st = "-" + st;
                }
                return st;
            }
        }
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Timer;
    }
}
