using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class CampHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly SavePlayerStateService savePlayerStateService;

        public CampHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData, SavePlayerStateService savePlayerStateService) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.savePlayerStateService = savePlayerStateService;
            logEvents.CampEvent += LogEvents_CampEvent;
        }

        private void LogEvents_CampEvent(object sender, CampEvent e)
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
