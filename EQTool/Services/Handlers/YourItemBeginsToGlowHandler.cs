using EQTool.Models;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class YourItemBeginsToGlowHandler : BaseHandler
    {
        private readonly EQSpells spells;
        private readonly SpellHandlerService baseSpellYouCastingHandler;

        public YourItemBeginsToGlowHandler(
            SpellHandlerService baseSpellYouCastingHandler,
            EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            logEvents.YourItemBeginsToGlow += LogEvents_YourItemBeginsToGlow;
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
