using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class FTEHandler : BaseHandler
    {
        public FTEHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.FTEEvent += LogParser_FTEEvent;
        }

        private void LogParser_FTEEvent(object sender, FTEEvent e)
        {
            if (activePlayer?.Player?.FTEAudio == true)
            {
                textToSpeach.Say($"{e.FTEPerson} F T E {e.NPCName}");
            }
        }
    }
}
