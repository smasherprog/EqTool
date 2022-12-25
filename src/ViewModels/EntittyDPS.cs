using EQTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace EQTool.ViewModels
{
    public class EntittyDPS : INotifyPropertyChanged
    {
        private string _SourceName = string.Empty;

        public string SourceName
        {
            get => _SourceName;
            set
            {
                _SourceName = value;
                BackGroundColor = _SourceName == EQSpells.SpaceYou ? Brushes.LightBlue : Brushes.DarkSeaGreen;
                OnPropertyChanged();
            }
        }

        public string SourceName_and_Info
        {
            get
            {
                var ret = _SourceName;
                if (this.DeathTime.HasValue)
                {
                    ret += "  (Dead)";
                }
                ret += $"    {TotalSeconds} sec";
                return ret;
            }
        }

        private string _TargetName = string.Empty;

        public string TargetName
        {
            get => _TargetName;
            set
            {
                _TargetName = value;
                BackGroundColor = _TargetName == EQSpells.SpaceYou ? Brushes.LightBlue : Brushes.DarkSeaGreen;
                OnPropertyChanged();
            }
        }

        public DateTime? DeathTime { get; set; }

        private DateTime _StartTime = DateTime.Now;

        public DateTime StartTime
        {
            get => _StartTime;
            set
            {
                _StartTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DPS));
                OnPropertyChanged(nameof(TotalSeconds));
            }
        }

        private int Last12secondIndex = 0;

        public void UpdateDps()
        {
            if (DeathTime.HasValue)
            {
                return;
            }

            if (Damage.Any())
            {
                TrailingDamage = Damage.Where(a => a.TimeStamp >= DateTime.Now.AddSeconds(-12)).Sum(a => a.Damage);
                Update12SecondDmg();
            }

            OnPropertyChanged(nameof(TargetTotalDamage));
            OnPropertyChanged(nameof(TotalTwelveSecondDamage));
            OnPropertyChanged(nameof(DPS));
            OnPropertyChanged(nameof(TotalDPS));
            OnPropertyChanged(nameof(SourceName_and_Info));
        }

        private void Update12SecondDmg()
        {
            var firsttimestamp = Damage.FirstOrDefault().TimeStamp;
            var endtimestamp = Damage.LastOrDefault().TimeStamp;
            if ((endtimestamp - firsttimestamp).TotalSeconds < 12)
            {
                TotalTwelveSecondDamage = TotalDamage;
            }
            else if (Last12secondIndex <= Damage.Count - 1)
            {
                var item = Damage[Last12secondIndex];
                while ((endtimestamp - item.TimeStamp).TotalSeconds > 12)
                {
                    item = Damage[Last12secondIndex];
                    var timestampstep = item.TimeStamp;
                    var highesttempdmg = GetDamangeAfter(Last12secondIndex, timestampstep);
                    TotalTwelveSecondDamage = Math.Max(TotalTwelveSecondDamage, highesttempdmg);
                    Last12secondIndex++;
                }
            }
        }

        private int GetDamangeAfter(int i, DateTime lasttimestamp)
        {
            var totaldamage = 0;
            for (var j = i; j < Damage.Count; j++)
            {
                var inneritem = Damage[j];
                var timedelta = Math.Abs((lasttimestamp - inneritem.TimeStamp).TotalMilliseconds);
                if (timedelta >= 12000)
                {
                    break;
                }
                totaldamage += inneritem.Damage;
            }

            return totaldamage;
        }

        public int TotalSeconds => DeathTime.HasValue ? (int)(DeathTime.Value - _StartTime).TotalSeconds : (int)(DateTime.Now - _StartTime).TotalSeconds;
        public DateTime? LastDamageDone => Damage.LastOrDefault()?.TimeStamp;

        public void AddDamage(DamagePerTime damage)
        {
            Damage.Add(damage);
            TotalDamage += damage.Damage;

            if (damage.Damage > HighestHit)
            {
                HighestHit = damage.Damage;
                OnPropertyChanged(nameof(HighestHit));
            }

            OnPropertyChanged(nameof(TotalDamage));
        }

        public class DamagePerTime
        {
            public DateTime TimeStamp { get; set; }

            public int Damage { get; set; }
        }

        private readonly List<DamagePerTime> Damage = new List<DamagePerTime>();

        public int TrailingDamage { get; private set; } = 0;

        public int TotalDamage { get; set; }

        private int _TargetTotalDamage;

        public int TargetTotalDamage
        {
            get => _TargetTotalDamage;
            set
            {
                _TargetTotalDamage = value;
                if (_TargetTotalDamage > 0)
                {
                    PercentOfTotalDamage = (int)(TotalDamage / (double)_TargetTotalDamage * 100.0);
                    ProgressBarColor = new SolidColorBrush(GetColorFromRedYellowGreenGradient(PercentOfTotalDamage));
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressBarColor));
                OnPropertyChanged(nameof(PercentOfTotalDamage));
            }
        }

        private static System.Windows.Media.Color GetColorFromRedYellowGreenGradient(double percentage)
        {
            var red = (percentage > 50 ? 1 - (2 * (percentage - 50) / 100.0) : 1.0) * 255;
            var green = (percentage > 50 ? 1.0 : 2 * percentage / 100.0) * 200;
            var blue = 0.0;
            var r = System.Windows.Media.Color.FromArgb(50, (byte)red, (byte)green, (byte)blue);
            return r;
        }

        public int TotalTwelveSecondDamage { get; set; }
        public int HighestHit { get; set; }
        public int PercentOfTotalDamage { get; set; }
        public SolidColorBrush BackGroundColor { get; set; }
        public SolidColorBrush ProgressBarColor { get; set; } = new SolidColorBrush(GetColorFromRedYellowGreenGradient(0));
        public int TotalDPS => (TotalDamage > 0 && TotalSeconds > 0) ? (int)(TotalDamage / (double)TotalSeconds) : 0;
        public int DPS => (TrailingDamage > 0) ? (int)(TrailingDamage / (double)12) : 0;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
