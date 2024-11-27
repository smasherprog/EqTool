using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class YourSpellInterupptedHandler : BaseHandler
    {
        private readonly IAppDispatcher appDispatcher;
        public YourSpellInterupptedHandler(IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.logEvents.YourSpellInterupptedEvent += LogEvents_YourSpellInterupptedEvent;
        }

        private void LogEvents_YourSpellInterupptedEvent(object sender, YourSpellInterupptedEvent e)
        {
            this.appDispatcher.DispatchUI(() =>
            { 
                activePlayer.UserCastingSpell = null;
            });
        }
    }
}
