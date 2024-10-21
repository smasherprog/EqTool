using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class CharmBreakHandler : BaseHandler
    {
        public CharmBreakHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.CharmBreakEvent += LogParser_CharmBreakEvent;
        }

        private void LogParser_CharmBreakEvent(object sender, CharmBreakEvent e)
        {
            if (activePlayer?.Player?.CharmBreakAudio == true)
            {
                textToSpeach.Say($"Charm Break");
            }
        }
    }
}
