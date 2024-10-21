using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class RootHasWornOffHandler : BaseHandler
    {
        public RootHasWornOffHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.SpellWornOffOtherEvent += LogParser_SpellWornOtherOffEvent;
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
            var overlay = activePlayer?.Player?.RootWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }

            if (RootSpells.Any(a => string.Equals(a, e.SpellName, StringComparison.OrdinalIgnoreCase)))
            {
                textToSpeach.Say($"{e.SpellName} has worn off!");
            }
        }
    }
}
