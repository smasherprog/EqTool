using EQTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
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

        public void UpdateDps(PlayerInfo player)
        {
            if (DeathTime.HasValue)
            {
                return;
            }

            TrailingDamage = Damage.Where(a => a.TimeStamp >= DateTime.Now.AddSeconds(-12)).Sum(a => a.Damage);
            TotalDamage = Damage.Sum(a => a.Damage);
            if (Damage.Any())
            {
                var timestampstep = Damage.FirstOrDefault().TimeStamp;
                var highesttempdmg = GetDamangeAfter(0, timestampstep);
                TotalTwelveSecondDamage = 0;
                TotalTwelveSecondDamage = Math.Max(TotalTwelveSecondDamage, highesttempdmg);

                for (var i = 0; i < Damage.Count; i++)
                {
                    var item = Damage[i];
                    var timedelta = Math.Abs((item.TimeStamp - timestampstep).TotalMilliseconds);
                    if (timedelta >= 1000)
                    {
                        timestampstep = item.TimeStamp;
                        highesttempdmg = GetDamangeAfter(i, timestampstep);
                        TotalTwelveSecondDamage = Math.Max(TotalTwelveSecondDamage, highesttempdmg);
                    }
                }
            }

            if (DPS < GetMinDpsToShow(player.Level))
            {
                ColumnVisiblity = Visibility.Collapsed;
            }
            else
            {
                ColumnVisiblity = Visibility.Visible;
            }

            OnPropertyChanged(nameof(TotalTwelveSecondDamage));
            OnPropertyChanged(nameof(TotalDamage));
            OnPropertyChanged(nameof(DPS));
        }

        private Visibility _ColumnVisiblity = Visibility.Visible;

        public Visibility ColumnVisiblity
        {
            get => _ColumnVisiblity;
            set
            {
                if (_ColumnVisiblity == value)
                {
                    return;
                }
                _ColumnVisiblity = value;
                OnPropertyChanged();
            }
        } 

        public static int GetMinDpsToShow(int level)
        {
            var mindpstoshow = ((level / 10) * 2) + 1;
            return mindpstoshow;
        }

        public int TotalSeconds => DeathTime.HasValue ? (int)(DeathTime.Value - _StartTime).TotalSeconds : (int)(DateTime.Now - _StartTime).TotalSeconds;
        public DateTime? LastDamageDone => Damage.LastOrDefault()?.TimeStamp;
        public void AddDamage(DamagePerTime damage, PlayerInfo playerInfo)
        {
            Damage.Add(damage);

            if (damage.Damage > HighestHit)
            {
                HighestHit = damage.Damage;
                OnPropertyChanged(nameof(HighestHit));
            }

            UpdateDps(playerInfo);
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

        public class DamagePerTime
        {
            public DateTime TimeStamp { get; set; }

            public int Damage { get; set; }
        }

        private readonly List<DamagePerTime> Damage = new List<DamagePerTime>();

        public int TrailingDamage { get; private set; } = 0;
        public int TotalDamage { get; set; }
        public int TotalTwelveSecondDamage { get; set; }
        public int HighestHit { get; set; }
        public SolidColorBrush BackGroundColor { get; set; }

        public int DPS => (TrailingDamage > 0 && TotalSeconds > 0) ? (int)(TrailingDamage / (double)12) : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
