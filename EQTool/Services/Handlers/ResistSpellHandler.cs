using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class ResistSpellHandler : BaseHandler
    {
        public ResistSpellHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            var doAlert = activePlayer?.Player?.ResistWarningAudio ?? false;
            var target = e.isYou ? "You " : "Your target ";
            var text = $"{target} resisted the {e.Spell.name} spell";
            if (doAlert)
            {
                textToSpeach.Say(text);
            }
            doAlert = activePlayer?.Player?.ResistWarningOverlay ?? false;
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
