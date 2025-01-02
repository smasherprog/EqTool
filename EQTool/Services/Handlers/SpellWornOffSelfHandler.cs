using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffSelfHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffSelfHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.SpellWornOffSelfEvent += LogEvents_SpellWornOffSelfEvent;
        }

        private void LogEvents_SpellWornOffSelfEvent(object sender, SpellWornOffSelfEvent e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(e.SpellNames);
        }
    }
}
