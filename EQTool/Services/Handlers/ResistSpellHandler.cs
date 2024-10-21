using EQTool.Models;
using EQTool.ViewModels;

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
            var overlay = activePlayer?.Player?.ResistWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }
            var target = e.isYou ? "You " : "Your target ";
            textToSpeach.Say($"{target} resisted the {e.Spell.name} spell");
        }
    }
}
