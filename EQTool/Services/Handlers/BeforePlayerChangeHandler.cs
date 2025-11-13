using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class BeforePlayerChangeHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly SavePlayerStateService savePlayerStateService;
        public BeforePlayerChangeHandler(BaseHandlerData baseHandlerData, SpellWindowViewModel spellWindowViewModel, SavePlayerStateService savePlayerStateService) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.savePlayerStateService = savePlayerStateService;
            logEvents.BeforePlayerChangedEvent += LogEvents_PayerChangedEvent;
        }

        private void LogEvents_PayerChangedEvent(object sender, BeforePlayerChangedEvent e)
        {
            spellWindowViewModel.TryRemoveAmbiguousSpellSelf(SpellHandlerService.IllusionSpellPartialNames);
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(SpellHandlerService.AllCharmSpells);
            savePlayerStateService.TrySaveYouSpellData();
            
            spellWindowViewModel.ClearYouSpells();
            appDispatcher.DispatchUI(() =>
            { 
                activePlayer.Location = null;
            }); 
        }
    }
}
