using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
                OnPropertyChanged();
            }
        }

        public bool IsDead = false;

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

        public void UpdateDps()
        {
            OnPropertyChanged(nameof(TotalTwelveSecondDamage));
            OnPropertyChanged(nameof(TwelveSecondDPS));
            OnPropertyChanged(nameof(TotalDamage));
            OnPropertyChanged(nameof(DPS));
        }


        public int TotalSeconds => (int)(DateTime.Now - _StartTime).TotalSeconds;

        public void AddDamage(DamagePerTime damage)
        {
            Damage.Add(damage);
            TotalTwelveSecondDamage = 0;
            var timestampstep = Damage.FirstOrDefault().TimeStamp;
            var highesttempdmg = GetDamangeAfter(0, timestampstep);
            TotalTwelveSecondDamage = Math.Max(TotalTwelveSecondDamage, highesttempdmg);

            for (var i = 0; i < Damage.Count; i++)
            {
                var item = Damage[i];
                if ((item.TimeStamp - timestampstep).TotalMilliseconds > 1000)
                {
                    highesttempdmg = GetDamangeAfter(i, timestampstep);
                    timestampstep = item.TimeStamp;
                    TotalTwelveSecondDamage = Math.Max(TotalTwelveSecondDamage, highesttempdmg);
                }
            }

            TotalDamage = Damage.Sum(a => a.Damage);
            UpdateDps();
        }

        private int GetDamangeAfter(int i, DateTime lasttimestamp)
        {
            var totaldamage = 0;
            for (var j = i; j < Damage.Count; j++)
            {
                var inneritem = Damage[j];
                if ((lasttimestamp - inneritem.TimeStamp).TotalMilliseconds > 12000)
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

        public int TotalDamage { get; set; }
        public int TotalTwelveSecondDamage { get; set; }

        public int TwelveSecondDPS => (TotalTwelveSecondDamage > 0) ? (int)(TotalTwelveSecondDamage / (double)12) : 0;
        public int DPS => (TotalDamage > 0 && TotalSeconds > 0) ? (int)(TotalDamage / (double)TotalSeconds) : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
