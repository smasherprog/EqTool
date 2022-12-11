using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;

namespace EQTool.ViewModels
{
    public class FightVisualzationViewModel : INotifyPropertyChanged
    {
        private readonly List<LineSeries> lineSeries = new List<LineSeries>();

        private readonly List<DPSParseMatch> DPSList = new List<DPSParseMatch>();

        private readonly Chart mcChart;

        public FightVisualzationViewModel(Chart mcChart)
        {
            this.mcChart = mcChart;
        }

        public void AddData(DPSParseMatch match)
        {
            if (match?.SourceName == null)
            {
                return;
            }

            var listseries = new List<ISeries>();
            var series = lineSeries.FirstOrDefault(a => a.Title.ToString().ToLower() == match.SourceName.ToLower());
            var list = new ObservableCollection<DPSParseMatch>();
            if (series == null)
            {
                var r = new Random();
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)r.Next(1, 255), (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
                var style = new System.Windows.Style
                {
                    TargetType = typeof(LineDataPoint),
                };
                style.Setters.Add(new Setter(LineSeries.OpacityProperty, 0.0));
                style.Setters.Add(new Setter(LineSeries.BackgroundProperty, brush));
                series = new LineSeries
                {
                    ItemsSource = list,
                    Title = match.SourceName,
                    DependentValuePath = nameof(DPSParseMatch.DamageDone),
                    IndependentValuePath = nameof(DPSParseMatch.TimeStamp),
                    DataPointStyle = style,
                };
                lineSeries.Add(series);
                mcChart.Series.Add(series);
            }
            else
            {
                list = series.ItemsSource as ObservableCollection<DPSParseMatch>;
            }

            match.SourceName = match.SourceName.ToLower();
            DPSList.Add(match);
            var trailing5seconds = DPSList.Where(a => a.SourceName == match.SourceName && a.TimeStamp >= DateTime.Now.AddMilliseconds(-12000)).Sum(a => (int?)a.DamageDone);
            if (trailing5seconds > 0)
            {
                var dps = (int)((double)trailing5seconds / 12);
                var lastitem = list.LastOrDefault();
                if (lastitem != null && (match.TimeStamp - lastitem.TimeStamp).TotalMilliseconds <= 1000)
                {
                    lastitem.DamageDone = dps;
                }
                else
                {
                    list.Add(match);
                }
            }
        }

        public void TargetDied(string targetdead)
        {
            if (string.IsNullOrWhiteSpace(targetdead))
            {
                return;
            }
            lineSeries.Clear();
            DPSList.Clear();
            mcChart.Series.Clear();
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
                var series = lineSeries.FirstOrDefault(a => a.Title.ToString().ToLower() == name);
                if (series != null)
                {
                    _ = mcChart.Series.Remove(series);
                    _ = lineSeries.Remove(series);
                }
            }

            if (!DPSList.Any())
            {
                mcChart.Series.Clear();
                lineSeries.Clear();
            }
        }
    }
}
