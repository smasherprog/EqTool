using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class SettingsTestRunOverlay
    {
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;
        private readonly Action[] actions = new Action[(int)OverlayTypes.MaxTypes];

        public SettingsTestRunOverlay(ActivePlayer activePlayer, LogParser logParser)
        {
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            actions[(int)OverlayTypes.FTEEvent] = FTEEvent;
            actions[(int)OverlayTypes.FTETimerEvent] = FTETimerEvent;
            actions[(int)OverlayTypes.DragonFearEvent] = DragonFearEvent;
            actions[(int)OverlayTypes.RootBreakEvent] = RootBreakEvent;
            actions[(int)OverlayTypes.RandomRollEvent] = RandomRollEvent;
            actions[(int)OverlayTypes.DeathLoopEvent] = DeathLoopEvent;
            actions[(int)OverlayTypes.ZlandicarEvent] = ZlandicarEvent;
        }

        public void RunTest(OverlayTypes overlayType)
        {
            if (activePlayer?.Player == null)
            {
                return;
            }
            ((App)System.Windows.Application.Current).OpenOverLayWindow();
            actions[(int)overlayType]?.Invoke();
        }

        private void RandomRollEvent()
        {
            var random = new Random();
            var randomgroup = random.Next(100, 1000);
            var r = random.Next(0, randomgroup);

            logParser.Push("**A Magic Die is rolled by Whitewitch.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            r = random.Next(0, randomgroup);
            logParser.Push("**A Magic Die is rolled by Huntor.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            r = random.Next(0, randomgroup);
            logParser.Push("**A Magic Die is rolled by Vasanle.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            r = random.Next(0, randomgroup);
            logParser.Push("**A Magic Die is rolled by Sanare.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            r = random.Next(0, randomgroup);
            logParser.Push("**A Magic Die is rolled by Sanare.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            r = random.Next(0, randomgroup);
            logParser.Push("**A Magic Die is rolled by Stigeon.", DateTime.Now);
            logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
        }
        private void DeathLoopEvent()
        {
            activePlayer.Player.DeathLoopOverlay = true;
            activePlayer.Player.DeathLoopAudio = true;
            logParser.Push("You have been slain by a brigand!", DateTime.Now);
            logParser.Push("You have been slain by a brigand!", DateTime.Now);
            logParser.Push("You have been slain by a brigand!", DateTime.Now);
            logParser.Push("You have been slain by a brigand!", DateTime.Now);
        }

        private void RootBreakEvent()
        {
            activePlayer.Player.RootWarningAudio = true;
            activePlayer.Player.RootWarningOverlay = true;
            logParser.Push("Your Paralyzing Earth spell has worn off.", DateTime.Now);
        }

        private void DragonFearEvent()
        {
            activePlayer.Player.DragonRoarAudio = true;
            activePlayer.Player.DragonRoarOverlay = true;
            logParser.Push($"You flee in terror.", DateTime.Now);
        }

        private void FTEEvent()
        {
            activePlayer.Player.FTEAudio = true;
            activePlayer.Player.FTEOverlay = true;
            logParser.Push("Dagarn the Destroyer engages Tzvia!", DateTime.Now);
        }
        private void FTETimerEvent()
        {
            activePlayer.Player.FTETimerAudio = true;
            activePlayer.Player.FTETimerOverlay = true;
            logParser.Push("Zlandicar engages Tzvia!", DateTime.Now);
        }

        private void ZlandicarEvent()
        {
            activePlayer.Player.ZlandicarOverlay = true;
            activePlayer.Player.ZlandicarAudio = true;
            var previousZone = activePlayer.Player.Zone;
            activePlayer.Player.Zone = "necropolis";
            logParser.Push($"You flee in terror.", DateTime.Now);
            _ = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                logParser.Push($"Your eardrums rupture.", DateTime.Now);
            });
        }
    }
}
