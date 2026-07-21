using EQTool.Models;
using EQTool.ViewModels;
using System;

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
            actions[(int)OverlayTypes.RootBreakEvent] = RootBreakEvent;
            actions[(int)OverlayTypes.RandomRollEvent] = RandomRollEvent;
            actions[(int)OverlayTypes.DeathLoopEvent] = DeathLoopEvent;
            actions[(int)OverlayTypes.AfkAttackedEvent] = AfkAttackedEvent;
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
            var playername = activePlayer?.Player?.Name;
            if (!string.IsNullOrWhiteSpace(playername))
            {
                r = random.Next(0, randomgroup);
                logParser.Push($"**A Magic Die is rolled by {playername}.", DateTime.Now);
                logParser.Push($"**It could have been any number from 0 to {randomgroup}, but this time it turned up a {r}.", DateTime.Now);
            }
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

        private void FTEEvent()
        {
            activePlayer.Player.FTEAudio = true;
            activePlayer.Player.FTEOverlay = true;
            logParser.Push("Dagarn the Destroyer engages Tzvia!", DateTime.Now);
        }
        private void AfkAttackedEvent()
        {
            activePlayer.Player.AfkAttackedAudio = true;
            activePlayer.Player.AfkAttackedOverlay = true;
            // The alert only fires while eqgame is not focused, which is the case when
            // testing from the settings window.
            logParser.Push("a giant rat hits YOU for 15 points of damage.", DateTime.Now);
        }
        private void FTETimerEvent()
        {
            activePlayer.Player.FTETimerAudio = true;
            activePlayer.Player.FTETimerOverlay = true;
            logParser.Push("Zlandicar engages Tzvia!", DateTime.Now);
        }
    }
}
