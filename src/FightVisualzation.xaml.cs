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
        private readonly ObservableCollection<KeyValuePair<DateTime, int>> lineSeries = new ObservableCollection<KeyValuePair<DateTime, int>>();
        public FightVisualzation()
        {
            InitializeComponent();
            ((LineSeries)mcChart.Series[0]).ItemsSource = lineSeries;
            var dpslist = new List<KeyValuePair<DateTime, int>>();
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
                        if (match?.SourceName == "A burning guardian")
                        {
                            dpslist.Add(new KeyValuePair<DateTime, int>(match.TimeStamp, match.DamageDone));
                        }
                    }
                }
            }

            var dpslist2 = new List<KeyValuePair<DateTime, int>>();
            foreach (var item in dpslist.GroupBy(a => a.Key))
            {
                dpslist2.Add(new KeyValuePair<DateTime, int>(item.Key, item.Sum(a => a.Value)));
            }

            foreach (var item in dpslist2)
            {
                lineSeries.Add(item);
            }
        }
    }
}
