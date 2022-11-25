using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for FightVisualzation.xaml
    /// </summary>
    public partial class FightVisualzation : Window
    {
        private readonly LogParser logParser;
        private readonly DPSLogParse dPSLogParse;
        private readonly FightVisualzationViewModel fightVisualzationViewModel;

        public FightVisualzation(LogParser logParser, DPSLogParse dPSLogParse)
        {
            InitializeComponent();
            this.logParser = logParser;
            this.dPSLogParse = dPSLogParse;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            fightVisualzationViewModel = new FightVisualzationViewModel(mcChart);
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = dPSLogParse.Match(e.Line);
            fightVisualzationViewModel.AddData(matched);
        }
    }
}
