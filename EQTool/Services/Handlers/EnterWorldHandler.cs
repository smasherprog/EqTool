using EQTool.Models;
using EQTool.Services.Factories;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class EnterWorldHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public EnterWorldHandler(
            SpellWindowViewModel spellWindowViewModel,
            SpellWindowViewModelFactory spellWindowViewModelFactory,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.CampEvent += LogEvents_CampEvent;
        }

        private void LogEvents_CampEvent(object sender, CampEvent e)
        {
            spellWindowViewModel.ClearYouSpells();
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
        }
    }
}
