using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class CampHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public CampHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.CampEvent += LogEvents_CampEvent;
        }

        private void LogEvents_CampEvent(object sender, CampEvent e)
        {
            TrySaveYouSpellData();
            spellWindowViewModel.ClearYouSpells();
            activePlayer.Location = null;
        }
        private void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                appDispatcher.DispatchUI(() =>
                {
                    var before = activePlayer.Player.YouSpells ?? new System.Collections.Generic.List<YouSpells>();
                    activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Where(a => a.GroupName == EQSpells.SpaceYou && a.SpellViewModelType == ViewModels.SpellWindow.SpellViewModelType.Spell)
                        .Cast<SpellViewModel>()
                        .Select(a => new YouSpells
                        {
                            Name = a.Name,
                            TotalSecondsLeft = (int)a.TotalRemainingDuration.TotalSeconds,
                        }).ToList();
                });
            }
        }
    }
}
