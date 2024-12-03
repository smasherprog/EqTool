using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerLocationHandler : BaseHandler
    {
        private readonly IAppDispatcher dispatcher;

        public PlayerLocationHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach, IAppDispatcher dispatcher) :
            base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.PlayerLocationEvent += LogEvents_PlayerLocationEvent;
            this.dispatcher = dispatcher;
        }

        private void LogEvents_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            dispatcher.DispatchUI(() =>
            {
                activePlayer.Location = e.Location;
            });
        }
    }
}
