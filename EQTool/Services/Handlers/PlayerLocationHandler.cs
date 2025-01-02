using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class PlayerLocationHandler : BaseHandler
    {
        public PlayerLocationHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.PlayerLocationEvent += LogEvents_PlayerLocationEvent;
        }

        private void LogEvents_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.Location = e.Location;
            });
        }
    }
}
