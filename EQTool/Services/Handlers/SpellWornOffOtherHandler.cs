using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffOtherHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffOtherHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            if (SpellHandlerService.SpellsThatNeedCounts.Contains(e.SpellName))
            {
                return;
            }

            // try and remove the matching spell from the timer list
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);

            // handle the case where we get the generic Charm Break message, but we'd like to remove the corresponding charm spell from the timer list
            if (e.Line == "Your charm spell has worn off.")
            {
                // go find the charm spell on the timer bar and remove it
                spellWindowViewModel.TryRemoveUnambiguousSpellOther(SpellHandlerService.AllCharmSpells);
                spellWindowViewModel.TryRemoveUnambiguousSpellSelf(SpellHandlerService.AllCharmSpells);
            }
        }
    }
}
