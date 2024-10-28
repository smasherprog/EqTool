using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SlainHandler : BaseHandler
    {
        public SlainHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
            this.logEvents.ExperienceGainedEvent += LogEvents_ExperienceGainedEvent;
        }

        private void LogEvents_ExperienceGainedEvent(object sender, ExperienceGainedEvent e)
        {

        }

        private void LogEvents_FactionEvent(object sender, FactionEvent e)
        {


        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {

        }

    }
}
