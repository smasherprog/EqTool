using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class EnrageHandler : BaseHandler
    {
        public EnrageHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.EnrageEvent += LogParser_EnrageEvent;
        }

        private void LogParser_EnrageEvent(object sender, EnrageEvent e)
        {
            if (activePlayer?.Player?.EnrageAudio == true)
            {
                textToSpeach.Say($"{e.NpcName} is enraged.");
            }
        }
    }
}
