using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnYouHandler : BaseHandler
    {
        private readonly SpellHandlerService baseSpellYouCastingHandler;
        private readonly CasterGuessingService casterGuessingService;

        public SpellCastOnYouHandler(
            SpellHandlerService baseSpellYouCastingHandler,
            CasterGuessingService casterGuessingService,
            BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            this.casterGuessingService = casterGuessingService;
            logEvents.SpellCastOnYouEvent += LogEvents_SpellCastOnYouEvent;
        }

        private void LogEvents_SpellCastOnYouEvent(object sender, SpellCastOnYouEvent e)
        {
            var targetName = EQSpells.SpaceYou;
            var casterName = casterGuessingService.TryGuessNameForTimer(e.Spell, targetName, requireCertainty: false);
            baseSpellYouCastingHandler.Handle(e.Spell, casterName, targetName, 0, e.TimeStamp);
        }
    }
}
