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
            actions[(int)OverlayTypes.HealedYou] = RunHealedYouTest;
            actions[(int)OverlayTypes.EnrageEvent] = EnrageEvent;
            actions[(int)OverlayTypes.LevitateEvent] = LevitateEvent;
            actions[(int)OverlayTypes.InvisEvent] = InvisEvent;
            actions[(int)OverlayTypes.FTEEvent] = FTEEvent;
            actions[(int)OverlayTypes.FTETimerEvent] = FTETimerEvent;
            actions[(int)OverlayTypes.CharmBreakEvent] = CharmBreakEvent;
            actions[(int)OverlayTypes.FailedFeignEvent] = FailedFeignEvent;
            actions[(int)OverlayTypes.GroupInviteEvent] = GroupInviteEvent;
            actions[(int)OverlayTypes.DragonFearEvent] = DragonFearEvent;
            actions[(int)OverlayTypes.RootBreakEvent] = RootBreakEvent;
            actions[(int)OverlayTypes.ResistSpellEvent] = ResistSpellEvent;
            actions[(int)OverlayTypes.RandomRollEvent] = RandomRollEvent;
            actions[(int)OverlayTypes.DeathLoopEvent] = DeathLoopEvent;
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

        private void RunHealedYouTest()
        {
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

        private void ResistSpellEvent()
        {
            activePlayer.Player.ResistWarningAudio = true;
            activePlayer.Player.ResistWarningOverlay = true;
            logParser.Push("Your target resisted the Rest the Dead spell.", DateTime.Now);
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

        private void GroupInviteEvent()
        {
            activePlayer.Player.GroupInviteAudio = true;
            activePlayer.Player.GroupInviteOverlay = true;
            logParser.Push($"Tzvia invites you to join a group.", DateTime.Now);
        }

        private void InvisEvent()
        {
            activePlayer.Player.InvisFadingAudio = true;
            activePlayer.Player.InvisFadingOverlay = true;
            logParser.Push("You feel yourself starting to appear.", DateTime.Now);
        }

        private void LevitateEvent()
        {
            activePlayer.Player.LevFadingAudio = true;
            activePlayer.Player.LevFadingOverlay = true;
            logParser.Push("You feel as if you are about to fall.", DateTime.Now);
        }

        private void CharmBreakEvent()
        {
            activePlayer.Player.CharmBreakAudio = true;
            activePlayer.Player.CharmBreakOverlay = true;
            logParser.Push("Your charm spell has worn off.", DateTime.Now);
        }

        private void FailedFeignEvent()
        {
            activePlayer.Player.FailedFeignAudio = true;
            activePlayer.Player.FailedFeignOverlay = true;
            logParser.Push($"{activePlayer.Player.Name} has fallen to the ground.", DateTime.Now);
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

        private void EnrageEvent()
        {
            activePlayer.Player.EnrageAudio = true;
            activePlayer.Player.EnrageOverlay = true;
            logParser.Push("Visceryn has become ENRAGED.", DateTime.Now);
        }
    }
}
