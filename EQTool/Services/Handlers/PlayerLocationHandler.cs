using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerLocationHandler : BaseHandler
    { 
        public PlayerLocationHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) :
            base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.PlayerLocationEvent += LogEvents_PlayerLocationEvent; 
        }

        private void LogEvents_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            this.activePlayer.Location = e.Location;
        }
    }
}
