using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{

    public class LevitateHandler : BaseHandler
    {
        public LevitateHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.LevitateEvent += LogParser_LevEvent;
        }

        private void LogParser_LevEvent(object sender, LevitateEvent e)
        {
            if (activePlayer?.Player?.LevFadingAudio == true && e.LevitateStatus == LevParser.LevStatus.Fading)
            {
                textToSpeach.Say("Levitate Fading");
            }
        }
    }
}
