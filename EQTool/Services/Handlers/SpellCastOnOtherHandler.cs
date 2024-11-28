using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnOtherHandler : BaseHandler
    {

        private readonly List<string> SpellsThatNeedCounts = new List<string>()
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
        };
         
        private readonly IAppDispatcher appDispatcher;
        private readonly BaseSpellYouCastingHandler baseSpellYouCastingHandler;
        private readonly SpellDurations spellDurations;

        public SpellCastOnOtherHandler(
            SpellDurations spellDurations, 
            LogEvents logEvents,
            BaseSpellYouCastingHandler baseSpellYouCastingHandler,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            IAppDispatcher appDispatcher,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.spellDurations = spellDurations;
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler; 
            this.logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            var userCastingSpell = this.activePlayer.UserCastingSpell;
            var userCastSpellDateTime = this.activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds >= userCastingSpell.casttime && e.Spells.Any(a => a.name == userCastingSpell.name))
                {
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.UserCastingSpell = null;
                        activePlayer.UserCastSpellDateTime = null;
                    });
                    this.baseSpellYouCastingHandler.Handle(userCastingSpell, e.TargetName, 0, e.TimeStamp);
                    return;
                }
                else
                {
                    userCastingSpell = null;
                    userCastSpellDateTime = null;
                }
            }

            if(userCastingSpell == null)
            {
                userCastingSpell = spellDurations.MatchClosestLevelToSpell(e.Spells, e.TimeStamp); 
                if(userCastingSpell == null)
                {
                    throw new Exception($"Could not match spell for '{e.Line}'");
                }
            }

            this.baseSpellYouCastingHandler.Handle(userCastingSpell, e.TargetName, 0, e.TimeStamp);
        }
    }
}
