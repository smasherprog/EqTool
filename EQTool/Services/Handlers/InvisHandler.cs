using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class InvisHandler : BaseHandler
    {
        public InvisHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.InvisEvent += LogParser_InvisEvent;
        }

        private void LogParser_InvisEvent(object sender, InvisEvent e)
        {
            if (activePlayer?.Player?.InvisFadingAudio == true && e.InvisStatus == InvisParser.InvisStatus.Fading)
            {
                textToSpeach.Say($"Invisability Fading.");
            }
        }
    }
}
