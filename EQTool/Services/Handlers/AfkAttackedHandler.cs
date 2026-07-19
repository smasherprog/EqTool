using EQTool.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    //
    // AfkAttackedHandler
    //
    // Warns the player when something is attacking THEM while eqgame does not have
    // focus (i.e. they have tabbed away and may be AFK). Fires a text-to-speech and/or
    // overlay alert based on the per-character AfkAttacked toggles.
    //
    public class AfkAttackedHandler : BaseHandler
    {
        // Throttle so a stream of incoming hits doesn't spam the overlay/voice.
        private static readonly TimeSpan AlertCooldown = TimeSpan.FromSeconds(5);
        private DateTime lastAlert = DateTime.MinValue;

        public AfkAttackedHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.DamageEvent += LogEvents_DamageEvent;
        }

        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            // Are WE the one being attacked? The EQ log writes the local player as "YOU".
            var attackedIsYou = string.Equals(e.TargetName, "YOU", StringComparison.OrdinalIgnoreCase);
            if (!attackedIsYou)
            {
                return;
            }

            // Ignore self-inflicted / non-melee events where we are recorded as the attacker.
            if (string.Equals(e.AttackerName, "You", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var audio = activePlayer?.Player?.AfkAttackedAudio ?? false;
            var overlay = activePlayer?.Player?.AfkAttackedOverlay ?? false;
            if (!audio && !overlay)
            {
                return;
            }

            // Only warn when eqgame is NOT the focused window.
            if (ForegroundWindowHelper.IsEqGameFocused())
            {
                return;
            }

            var now = e.TimeStamp == default ? DateTime.Now : e.TimeStamp;
            if (now - lastAlert < AlertCooldown)
            {
                return;
            }
            lastAlert = now;

            var attacker = string.IsNullOrWhiteSpace(e.AttackerName) ? "something" : e.AttackerName;

            if (audio)
            {
                textToSpeach.Say("You are being attacked");
            }

            if (overlay)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    var text = $"AFK - You are being attacked by {attacker}";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
