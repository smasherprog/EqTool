using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Timers;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for FightVisualzation.xaml
    /// </summary>
    public partial class FightVisualzation : Window
    {
        private readonly Timer UITimer;
        private readonly LogParser logParser;
        private readonly DPSLogParse dPSLogParse;
        private readonly FightVisualzationViewModel fightVisualzationViewModel;
        private readonly LogDeathParse logDeathParse;
        private readonly IAppDispatcher appDispatcher;

        public FightVisualzation(LogParser logParser, DPSLogParse dPSLogParse, LogDeathParse logDeathParse, IAppDispatcher appDispatcher)
        {
            InitializeComponent();
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            this.appDispatcher = appDispatcher;
            this.logDeathParse = logDeathParse;
            this.logParser = logParser;
            this.dPSLogParse = dPSLogParse;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            fightVisualzationViewModel = new FightVisualzationViewModel(mcChart);
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = dPSLogParse.Match(e.Line);
            fightVisualzationViewModel.AddData(matched);
            var targetdead = logDeathParse.GetDeadTarget(e.Line);
            fightVisualzationViewModel.TargetDied(targetdead);
        }

        private void PollUI(object sender, EventArgs e)
        {
            appDispatcher.DispatchUI(() =>
            {
                fightVisualzationViewModel.UpdateDPS();
            });
        }
    }
}
