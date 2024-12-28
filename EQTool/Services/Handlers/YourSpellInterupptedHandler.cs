using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class YourSpellInterupptedHandler : BaseHandler
    {
        public YourSpellInterupptedHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.YourSpellInterupptedEvent += LogEvents_YourSpellInterupptedEvent;
        }

        private void LogEvents_YourSpellInterupptedEvent(object sender, YourSpellInterupptedEvent e)
        {
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = null;
                activePlayer.UserCastSpellDateTime = null;
            });
        }
    }
}
