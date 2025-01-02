using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class RootHasWornOffHandler : BaseHandler
    {
        public RootHasWornOffHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.SpellWornOffOtherEvent += LogParser_SpellWornOtherOffEvent;
        }

        private readonly List<string> RootSpells = new List<string>()
        {
            "Root",
            "Fetter",
            "Enstill",
            "Immobalize",
            "Paralyzing Earth",
            "Grasping Roots",
            "Ensnaring Roots",
            "Enveloping Roots",
            "Engulfing Roots",
            "Engorging Roots",
            "Entrapping Roots"
        };

        private void LogParser_SpellWornOtherOffEvent(object sender, SpellWornOffOtherEvent e)
        {
            if (!RootSpells.Any(a => string.Equals(a, e.SpellName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            var doAlert = activePlayer?.Player?.RootWarningAudio ?? false;
            var text = $"{e.SpellName} has worn off!";
            if (doAlert)
            {
                textToSpeach.Say(text);
            }
            doAlert = activePlayer?.Player?.RootWarningOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
