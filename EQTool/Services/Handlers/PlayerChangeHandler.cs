using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerChangeHandler : BaseHandler
    { 
        public PlayerChangeHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) :
            base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            this.activePlayer.Location = null;
        } 
    }
}
