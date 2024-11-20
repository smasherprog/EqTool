using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class CampHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly IAppDispatcher dispatcher;

        public CampHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            IAppDispatcher dispatcher,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.dispatcher = dispatcher;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.CampEvent += LogEvents_CampEvent;
        }

        private void LogEvents_CampEvent(object sender, CampEvent e)
        {
            TrySaveYouSpellData();
            spellWindowViewModel.ClearYouSpells();
        }
        private void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                dispatcher.DispatchUI(() =>
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
