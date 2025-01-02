using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class YouBeginCastingHandler : BaseHandler
    {
        public YouBeginCastingHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        {
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = e.Spell;
                activePlayer.UserCastSpellDateTime = e.TimeStamp;
            });
        }
    }
}
