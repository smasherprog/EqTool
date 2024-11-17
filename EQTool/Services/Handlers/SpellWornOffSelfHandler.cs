using EQTool.Models;
using EQTool.Services.Factories;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffSelfHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffSelfHandler(
            SpellWindowViewModel spellWindowViewModel,
            SpellWindowViewModelFactory spellWindowViewModelFactory,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SpellWornOffSelfEvent += LogEvents_SpellWornOffSelfEvent; ;
        }

        private void LogEvents_SpellWornOffSelfEvent(object sender, SpellWornOffSelfEvent e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }
    }
}
