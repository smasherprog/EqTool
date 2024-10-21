using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class FailedFeignHandler : BaseHandler
    {
        public FailedFeignHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.FailedFeignEvent += LogParser_FailedFeignEvent;
        }

        private void LogParser_FailedFeignEvent(object sender, FailedFeignEvent e)
        {
            if (activePlayer?.Player?.FailedFeignAudio == true)
            {
                textToSpeach.Say($"{e.PersonWhoFailedFeign} Failed Feign Death");
            }
        }
    }
}
