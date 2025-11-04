using EQTool.Models;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class YourItemBeginsToGlowHandler : BaseHandler
    {
        private readonly EQSpells spells;

        public YourItemBeginsToGlowHandler(EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            logEvents.YourItemBeginsToGlow += LogEvents_YourItemBeginsToGlow;
        }

        private void LogEvents_YourItemBeginsToGlow(object sender, YourItemBeginsToGlow e)
        {
            if (e.ItemName == "Pegasus Feather Cloak" && spells.AllSpells.TryGetValue("Peggy Levitate", out var foundSpell))
            {
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.UserCastingSpell = foundSpell;
                    activePlayer.UserCastSpellDateTime = e.TimeStamp;
                });
            }
        }
    }
}
