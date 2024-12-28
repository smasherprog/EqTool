using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class BeginsToCastTheGateSpellHandler : BaseHandler
    {
        public BeginsToCastTheGateSpellHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.logEvents.NPCBeginsToGateEvent += LogEvents_NPCBeginsToGateEvent;
        }

        private void LogEvents_NPCBeginsToGateEvent(object sender, NPCBeginsToGateEvent e)
        {
            var text = $"{e.NPCName} begins to Gate";
            if (activePlayer?.Player?.MobGatingAudio == true)
            {
                textToSpeach.Say(text);
            }

            var doAlert = activePlayer?.Player?.MobGatingOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
