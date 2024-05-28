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
            actions[(int)OverlayTypes.CharmBreakEvent] = CharmBreakEvent;
            actions[(int)OverlayTypes.FailedFeignEvent] = FailedFeignEvent;
            actions[(int)OverlayTypes.GroupInviteEvent] = GroupInviteEvent;
            actions[(int)OverlayTypes.DragonFearEvent] = DragonFearEvent;
            actions[(int)OverlayTypes.RootBreakEvent] = RootBreakEvent;
            actions[(int)OverlayTypes.ResistSpellEvent] = ResistSpellEvent;
            actions[(int)OverlayTypes.RandomRollEvent] = RandomRollEvent;
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

        private void PushLog(string message)
        {
            var logtext = message?.Trim();
            if (string.IsNullOrWhiteSpace(logtext))
            {
                return;
            }
            if (!logtext.StartsWith("["))
            {
                var format = "ddd MMM dd HH:mm:ss yyyy";
                var d = DateTime.Now;
                logtext = "[" + d.ToString(format) + "] " + logtext;
            }
            logParser.Push(logtext);
        }

        private void RunHealedYouTest()
        {
        }

        private void RandomRollEvent()
        {
            var random = new Random();
            var r = random.Next(0, 333);
            PushLog("**A Magic Die is rolled by Whitewitch.");
            PushLog($"**It could have been any number from 0 to 333, but this time it turned up a {r}.");
            r = random.Next(0, 333);
            PushLog("**A Magic Die is rolled by Huntor.");
            PushLog($"**It could have been any number from 0 to 333, but this time it turned up a {r}.");
            r = random.Next(0, 333);
            PushLog("**A Magic Die is rolled by Vasanle.");
            PushLog($"**It could have been any number from 0 to 333, but this time it turned up a {r}.");
            r = random.Next(0, 333);
            PushLog("**A Magic Die is rolled by Sanare.");
            PushLog($"**It could have been any number from 0 to 333, but this time it turned up a {r}.");
        }

        private void ResistSpellEvent()
        {
            activePlayer.Player.ResistWarningAudio = true;
            activePlayer.Player.ResistWarningOverlay = true;
            PushLog("Your target resisted the Rest the Dead spell.");
        }

        private void RootBreakEvent()
        {
            activePlayer.Player.RootWarningAudio = true;
            activePlayer.Player.RootWarningOverlay = true;
            PushLog("Your Paralyzing Earth spell has worn off.");
        }

        private void DragonFearEvent()
        {
            activePlayer.Player.DragonRoarAudio = true;
            activePlayer.Player.DragonRoarOverlay = true;
            PushLog($"You flee in terror.");
        }

        private void GroupInviteEvent()
        {
            activePlayer.Player.GroupInviteAudio = true;
            activePlayer.Player.GroupInviteOverlay = true;
            PushLog($"Tzvia invites you to join a group.");
        }

        private void InvisEvent()
        {
            activePlayer.Player.InvisFadingAudio = true;
            activePlayer.Player.InvisFadingOverlay = true;
            PushLog("You feel yourself starting to appear.");
        }

        private void LevitateEvent()
        {
            activePlayer.Player.LevFadingAudio = true;
            activePlayer.Player.LevFadingOverlay = true;
            PushLog("You feel as if you are about to fall.");
        }

        private void CharmBreakEvent()
        {
            activePlayer.Player.CharmBreakAudio = true;
            activePlayer.Player.CharmBreakOverlay = true;
            PushLog("Your charm spell has worn off.");
        }

        private void FailedFeignEvent()
        {
            activePlayer.Player.FailedFeignAudio = true;
            activePlayer.Player.FailedFeignOverlay = true;
            PushLog($"{activePlayer.Player.Name} has fallen to the ground.");
        }

        private void FTEEvent()
        {
            activePlayer.Player.FTEAudio = true;
            activePlayer.Player.FTEOverlay = true;
            PushLog("Dagarn the Destroyer engages Tzvia!");
        }

        private void EnrageEvent()
        {
            activePlayer.Player.EnrageAudio = true;
            activePlayer.Player.EnrageOverlay = true;
            PushLog("Visceryn has become ENRAGED.");
        }
    }
}
