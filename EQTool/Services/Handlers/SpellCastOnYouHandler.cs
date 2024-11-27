using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnYouHandler : BaseHandler
    {  
        private readonly BaseSpellYouCastingHandler baseSpellYouCastingHandler;

        public SpellCastOnYouHandler( 
            BaseSpellYouCastingHandler baseSpellYouCastingHandler,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        { 
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            this.logEvents.SpellCastOnYouEvent += LogEvents_SpellCastOnYouEvent;
        }

        private void LogEvents_SpellCastOnYouEvent(object sender, SpellCastOnYouEvent e)
        {
            this.baseSpellYouCastingHandler.Handle(e.Spell, EQSpells.SpaceYou, 0, e.TimeStamp); 
        }
    }
}
