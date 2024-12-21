using EQTool.Models;
using EQTool.ViewModels;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class YourItemBeginsToGlowHandler : BaseHandler
    {
        private readonly IAppDispatcher appDispatcher;
        private readonly EQSpells spells;
        private readonly BaseSpellYouCastingHandler baseSpellYouCastingHandler;

        public YourItemBeginsToGlowHandler(
            IAppDispatcher appDispatcher,
            BaseSpellYouCastingHandler baseSpellYouCastingHandler,
            LogEvents logEvents,
            EQSpells spells,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spells = spells;
            this.appDispatcher = appDispatcher;
            this.logEvents.YourItemBeginsToGlow += LogEvents_YourItemBeginsToGlow;
        }

        private void LogEvents_YourItemBeginsToGlow(object sender, YourItemBeginsToGlow e)
        {
            if (e.ItemName == "Pegasus Feather Cloak")
            {
                var foundspell = spells.AllSpells.FirstOrDefault(a => a.name == "Peggy Levitate");
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.UserCastingSpell = foundspell;
                    activePlayer.UserCastSpellDateTime = e.TimeStamp;
                });
            }
        }
    }
}
