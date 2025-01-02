using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnYouHandler : BaseHandler
    {
        private readonly SpellHandlerService baseSpellYouCastingHandler;

        public SpellCastOnYouHandler(
            SpellHandlerService baseSpellYouCastingHandler, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            logEvents.SpellCastOnYouEvent += LogEvents_SpellCastOnYouEvent;
        }

        private void LogEvents_SpellCastOnYouEvent(object sender, SpellCastOnYouEvent e)
        {
            baseSpellYouCastingHandler.Handle(e.Spell, EQSpells.SpaceYou, 0, e.TimeStamp);
        }
    }
}
