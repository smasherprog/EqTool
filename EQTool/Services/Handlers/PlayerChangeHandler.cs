using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class PlayerChangeHandler : BaseHandler
    {
        public PlayerChangeHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            activePlayer.Location = null;
        }
    }
}
