using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class YourSpellInterruptedHandler : BaseHandler
    {
        public YourSpellInterruptedHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.YourSpellInteruptedEvent += LogEvents_YourSpellInterruptedEvent;
        }

        private void LogEvents_YourSpellInterruptedEvent(object sender, YourSpellInterupptedEvent e)
        {
            activePlayer.ClearUserCastingSpellImmediately();
        }
    }
}
