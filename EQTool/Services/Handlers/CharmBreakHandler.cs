using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class CharmBreakHandler : BaseHandler
    {
        public CharmBreakHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.CharmBreakEvent += LogParser_CharmBreakEvent;
        }

        private void LogParser_CharmBreakEvent(object sender, CharmBreakEvent e)
        {
            if (activePlayer?.Player?.CharmBreakAudio == true)
            {
                textToSpeach.Say($"Charm Break");
            }

            var doAlert = activePlayer?.Player?.CharmBreakOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var text = "Charm Break";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
