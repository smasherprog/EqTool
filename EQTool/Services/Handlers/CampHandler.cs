using EQTool.Models;
using EQTool.Services.Factories;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class CampHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public CampHandler(
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
            //TrySaveYouSpellData();
            //base.SaveState();
            spellWindowViewModel.ClearYouSpells();
        }
        private void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                //var before = activePlayer.Player.YouSpells ?? new System.Collections.Generic.List<YouSpells>();
                //activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Where(a => a.GroupName == EQSpells.SpaceYou).Select(a => new YouSpells
                //{
                //    Name = a.Name,
                //    TotalSecondsLeft = (int)a.TotalRemainingDuration.TotalSeconds,
                //}).ToList();
            }
        }
    }
}
