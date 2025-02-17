using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class WelcomeHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public WelcomeHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.WelcomeEvent += LogEvents_WelcomeEvent;
        }

        private void LogEvents_WelcomeEvent(object sender, WelcomeEvent e)
        {
            this.appDispatcher.DispatchUI(() =>
            {
                spellWindowViewModel.ClearYouSpells();
                if (activePlayer.Player != null)
                {
                    spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
                    activePlayer.Player.YouSpells.Clear();
                }
            });
        } 
    }
}
