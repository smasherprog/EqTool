using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly LogEvents logEvents;
        protected readonly ActivePlayer activePlayer;
        protected readonly EQToolSettings eQToolSettings;
        protected readonly ITextToSpeach textToSpeach;

        public BaseHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
            this.eQToolSettings = eQToolSettings;
            this.textToSpeach = textToSpeach;
        }
    }
}
