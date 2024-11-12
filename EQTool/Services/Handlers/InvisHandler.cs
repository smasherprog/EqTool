using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class InvisHandler : BaseHandler
    {
        public InvisHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.InvisEvent += LogParser_InvisEvent;
        }

        private void LogParser_InvisEvent(object sender, InvisEvent e)
        {
            if (e.InvisStatus != InvisParser.InvisStatus.Fading)
            {
                return;
            }
            var text = $"Invisability Fading.";
            if (activePlayer?.Player?.InvisFadingAudio == true)
            {
                textToSpeach.Say(text);
            }

            var doAlert = activePlayer?.Player?.InvisFadingOverlay ?? false;
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
