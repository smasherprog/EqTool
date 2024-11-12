using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class DragonRoarHandler : BaseHandler
    {
        public DragonRoarHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.SpellCastEvent += LogParser_StartCastingEvent;
        }

        private void LogParser_StartCastingEvent(object sender, SpellCastEvent e)
        {
            if (e.Spell.name != "Dragon Roar")
            {
                return;
            }
            var doAlert = activePlayer?.Player?.DragonRoarAudio ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    textToSpeach.Say($"Dragon Roar out");
                    System.Threading.Thread.Sleep(1000 * 30);
                    textToSpeach.Say($"Dragon Roar in 6 Seconds!");
                    System.Threading.Thread.Sleep(2000);
                    textToSpeach.Say($"4 Seconds!");
                    System.Threading.Thread.Sleep(2000);
                    textToSpeach.Say($"2");
                    System.Threading.Thread.Sleep(1000);
                    textToSpeach.Say($"1");
                });
            }

            doAlert = activePlayer?.Player?.DragonRoarOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000 * 30);
                    var text = "Dragon Roar in 6 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);

                    text = "Dragon Roar in 5 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);

                    text = "Dragon Roar in 4 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);

                    text = "Dragon Roar in 3 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);

                    text = "Dragon Roar in 2 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);

                    text = "Dragon Roar in 1 Seconds!";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(1000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
