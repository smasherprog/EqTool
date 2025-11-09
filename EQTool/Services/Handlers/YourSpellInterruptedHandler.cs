using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class YourSpellInterruptedHandler : BaseHandler
    {
        public YourSpellInterruptedHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.YourSpellInterruptedEvent += LogEvents_YourSpellInterruptedEvent;
        }

        private void LogEvents_YourSpellInterruptedEvent(object sender, YourSpellInterruptedEvent e)
        {
            activePlayer.ClearUserCastingSpellImmediately();
        }
    }
}
