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
        public class FightDataPoint
        {
            public DateTime X { get; set; }

            public int Y { get; set; }
        }
        private readonly List<LineSeries> lineSeries = new List<LineSeries>();

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
            var series = lineSeries.FirstOrDefault(a => a.Title.ToString() == match.SourceName);
            var list = new ObservableCollection<FightDataPoint>();
            if (series == null)
            {
                series = new LineSeries
                {
                    ItemsSource = list,
                    Title = match.SourceName,
                    DependentValuePath = "Y",
                    IndependentValuePath = "X"
                };
                lineSeries.Add(series);
                mcChart.Series.Add(series);
            }
            else
            {
                list = series.ItemsSource as ObservableCollection<FightDataPoint>;
            }

            list.Add(new FightDataPoint
            {
                X = match.TimeStamp,
                Y = match.DamageDone
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
