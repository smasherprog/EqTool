using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SlainHandler : BaseHandler
    {
        public SlainHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {

        }

    }
}
