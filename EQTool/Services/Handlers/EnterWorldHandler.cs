using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class EnterWorldHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public EnterWorldHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.EnteredWorldEvent += LogEvents_EnteredWorldEvent; ;
        }

        private void LogEvents_EnteredWorldEvent(object sender, EnteredWorldEvent e)
        {
            spellWindowViewModel.ClearYouSpells();
            if (activePlayer.Player != null)
            {
                spellWindowViewModel.AddSavedYouSpells(activePlayer.Player.YouSpells);
            }
        }
    }
}
