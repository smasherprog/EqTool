using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffOtherHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffOtherHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
        }
    }
}
