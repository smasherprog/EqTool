using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class FailedFeignHandler : BaseHandler
    {
        public FailedFeignHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.FailedFeignEvent += LogParser_FailedFeignEvent;
        }

        private void LogParser_FailedFeignEvent(object sender, FailedFeignEvent e)
        {
            if (activePlayer?.Player?.FailedFeignAudio == true)
            {
                textToSpeach.Say($"{e.PersonWhoFailedFeign} Failed Feign Death");
            }
            var doAlert = activePlayer?.Player?.FailedFeignOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var text = $"{e.PersonWhoFailedFeign} Feign Failed Death!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
