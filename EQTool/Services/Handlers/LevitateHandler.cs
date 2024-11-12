using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{

    public class LevitateHandler : BaseHandler
    {
        public LevitateHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.LevitateEvent += LogParser_LevEvent;
        }

        private void LogParser_LevEvent(object sender, LevitateEvent e)
        {
            if (e.LevitateStatus != LevParser.LevStatus.Fading)
            {
                return;
            }

            var text = "Levitate Fading";
            if (activePlayer?.Player?.LevFadingAudio == true)
            {
                textToSpeach.Say(text);
            }
            var doAlert = activePlayer?.Player?.LevFadingOverlay ?? false;
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
