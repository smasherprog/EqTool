using EQTool.Models;
using EQTool.ViewModels;

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
            var overlay = activePlayer?.Player?.DragonRoarAudio ?? false;
            if (!overlay || e.Spell.name != "Dragon Roar")
            {
                return;
            }

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
    }
}
