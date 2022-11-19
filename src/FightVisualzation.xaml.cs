using EQTool.Models;
using EQTool.Services.Spells.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for FightVisualzation.xaml
    /// </summary>
    public partial class FightVisualzation : Window
    {

        private readonly ObservableCollection<ISeries> Data = new ObservableCollection<ISeries>();

        public class FightDataPoint
        {
            public DateTime X { get; set; }

            public int Y { get; set; }
        }

        public FightVisualzation()
        {
            InitializeComponent();
            DataContext = this;
            var dpslist = new List<DPSParseMatch>();
            var dpsstuff = new DPSLogParse();
            using (var stream = new FileStream(@"C:\Users\smash\source\repos\smasherprog\EqTool\src\TestFight.txt", FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.Length > 27)
                    {
                        var match = dpsstuff.Match(line);
                        if (!string.IsNullOrWhiteSpace(match?.SourceName))
                        {
                            dpslist.Add(match);
                        }
                    }
                }
            }

            var listseries = new List<ISeries>();
            foreach (var sourcegroups in dpslist.GroupBy(a => a.SourceName))
            {
                var list = new List<FightDataPoint>();
                foreach (var timegroups in sourcegroups.GroupBy(a => a.TimeStamp))
                {
                    list.Add(new FightDataPoint
                    {
                        X = timegroups.Key,
                        Y = timegroups.Sum(a => a.DamageDone)
                    });
                }

                var lineseries = new LineSeries()
                {
                    ItemsSource = list,
                    Title = sourcegroups.Key,
                    DependentValuePath = "Y",
                    IndependentValuePath = "X"
                };
                mcChart.Series.Add(lineseries);
            }
        }
    }
}
