using EQTool.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class FightVisualzationViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ISeries> LineSeries
        {
            get => _LineSeries;
            set
            {
                _LineSeries = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ISeries> _LineSeries;

        public FightVisualzationViewModel()
        {
            _LineSeries = new ObservableCollection<ISeries>()
            {
                 new LineSeries<DateTimePoint>
                {
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.PrimaryValue} dps",
                    Values = new ObservableCollection<DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(2021, 1, 1, 1, 1, 1), 3),
                        new DateTimePoint(new DateTime(2021, 1, 1, 1, 1, 10), 8),
                        new DateTimePoint(new DateTime(2021, 1, 1, 1, 1, 20), 11),
                        new DateTimePoint(new DateTime(2021, 1, 1, 1, 1, 30), 15),
                        new DateTimePoint(new DateTime(2021, 1, 1, 1, 1, 40), 20),
                    }
                }
            };
        }

        private readonly List<DPSParseMatch> DPSList = new List<DPSParseMatch>();

        public void AddData(DPSParseMatch match)
        {
            if (match?.SourceName == null)
            {
                return;
            }

            var series = LineSeries.FirstOrDefault(a => a.Name.ToLower() == match.SourceName.ToLower());
            var list = new ObservableCollection<DateTimePoint>();
            if (series == null)
            {
                series = new LineSeries<DateTimePoint>
                {
                    Values = list,
                    Name = match.SourceName
                };
                LineSeries.Add(series);
            }
            else
            {
                // list = series.ItemsSource as ObservableCollection<DPSParseMatch>;
            }

            match.SourceName = match.SourceName.ToLower();
            DPSList.Add(match);
            var trailing5seconds = DPSList.Where(a => a.SourceName == match.SourceName && a.TimeStamp >= DateTime.Now.AddMilliseconds(-12000)).Sum(a => (int?)a.DamageDone);
            if (trailing5seconds > 0)
            {
                var dps = (int)((double)trailing5seconds / 12);
                var lastitem = list.LastOrDefault();
                //if (lastitem != null && (match.TimeStamp - lastitem.TimeStamp).TotalMilliseconds <= 1000)
                //{
                //    lastitem.DamageDone = dps;
                //}
                //else
                //{
                //    list.Add(match);
                //}
            }
        }

        public void TargetDied(string targetdead)
        {
            if (string.IsNullOrWhiteSpace(targetdead))
            {
                return;
            }
            LineSeries.Clear();
            DPSList.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateDPS()
        {
            var grpdps = DPSList.GroupBy(a => a.SourceName);
            var namestormove = new List<string>();
            foreach (var item in grpdps)
            {
                var lasttimestamp = item.LastOrDefault(a => !a.FakeAdd);
                if ((DateTime.Now - lasttimestamp.TimeStamp).TotalMilliseconds > 30000)
                {
                    namestormove.Add(item.Key);
                }
                else
                {
                    AddData(new DPSParseMatch
                    {
                        FakeAdd = true,
                        DamageDone = 0,
                        SourceName = item.Key,
                        TargetName = "Fake",
                        TimeStamp = DateTime.Now
                    });
                }
            }

            foreach (var item in namestormove)
            {
                var name = item.ToLower();
                _ = DPSList.RemoveAll(a => a.SourceName == name);
                var series = LineSeries.FirstOrDefault(a => a.Name.ToLower() == name);
                if (series != null)
                {
                    _ = LineSeries.Remove(series);
                }
            }

            if (!DPSList.Any())
            {
                //LineSeries.Clear();
            }
        }

        public Axis[] XAxes { get; set; } =
         {
                new Axis
                {
                    Labeler = value => new DateTime((long) value).ToString("h:mm:ss"),
                    LabelsRotation = 15,
                    UnitWidth = TimeSpan.FromHours(1).Ticks,
                    MinStep = TimeSpan.FromHours(1).Ticks
                }
            };
    }
}
