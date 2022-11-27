using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls.DataVisualization.Charting;


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
                series = new LineSeries
                {
                    ItemsSource = list,
                    Title = match.SourceName,
                    DependentValuePath = nameof(DPSParseMatch.DamageDone),
                    IndependentValuePath = nameof(DPSParseMatch.TimeStamp)
                };
                lineSeries.Add(series);
                mcChart.Series.Add(series);
            }
            else
            {
                list = series.ItemsSource as ObservableCollection<DPSParseMatch>;
            }

            match.TargetName = match.TargetName.ToLower();
            match.SourceName = match.SourceName.ToLower();
            DPSList.Add(match);
            var trailing5seconds = DPSList.Where(a => a.TargetName == match.TargetName && a.SourceName == match.SourceName && a.TimeStamp >= DateTime.Now.AddMilliseconds(-5000)).Sum(a => (int?)a.DamageDone);
            if (trailing5seconds > 0)
            {
                var dps = (int)((double)trailing5seconds / 5);
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
            targetdead = targetdead.ToLower();
            _ = DPSList.RemoveAll(a => a.TargetName == targetdead || a.SourceName == targetdead);
            var series = lineSeries.FirstOrDefault(a => a.Title.ToString().ToLower() == targetdead);
            if (series != null)
            {
                _ = mcChart.Series.Remove(series);
            }
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
                var listthing = item.OrderByDescending(a => a.TimeStamp).ToList();
                var lasttimestamp = listthing.FirstOrDefault();
                if ((DateTime.Now - lasttimestamp.TimeStamp).TotalMilliseconds > 30000)
                {
                    namestormove.Add(item.Key);
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
                }
            }

            if (!DPSList.Any())
            {
                mcChart.Series.Clear();
            }
        }
    }
}
