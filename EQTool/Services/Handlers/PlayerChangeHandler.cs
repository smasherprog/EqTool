using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerChangeHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly SavePlayerStateService savePlayerStateService;
        public PlayerChangeHandler(BaseHandlerData baseHandlerData, SpellWindowViewModel spellWindowViewModel, SavePlayerStateService savePlayerStateService) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.savePlayerStateService = savePlayerStateService;
            logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            this.savePlayerStateService.TrySaveYouSpellData();
            spellWindowViewModel.ClearYouSpells();
            appDispatcher.DispatchUI(() =>
            { 
                activePlayer.Location = null;
            }); 
        }
    }
}
