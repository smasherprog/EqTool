using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class EnrageHandler : BaseHandler
    {
        public EnrageHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.EnrageEvent += LogParser_EnrageEvent;
        }

        private void LogParser_EnrageEvent(object sender, EnrageEvent e)
        {
            var text = $"{e.NpcName} is enraged.";
            if (activePlayer?.Player?.EnrageAudio == true)
            {
                textToSpeach.Say(text);
            }
            var doAlert = activePlayer?.Player?.EnrageOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    text = $"{e.NpcName} ENRAGED";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000 * 12);
                    text = $"{e.NpcName} ENRAGE OFF";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
