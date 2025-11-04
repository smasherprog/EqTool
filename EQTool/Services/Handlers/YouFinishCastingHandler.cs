using System.Collections.Generic;
using System.Linq;
using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class YouFinishCastingHandler : BaseHandler
    {
        private readonly List<string> SelfSpellsThatDontEmitCompletionLogMesssages = new List<string>(
            SpellHandlerService.AllCharmSpells.Concat(new [] {
            "Harmshield",
            "Divine Aura",
            "Harmony"
        }));
        private readonly SpellHandlerService baseSpellYouCastingHandler;

        public YouFinishCastingHandler(SpellHandlerService baseSpellYouCastingHandler, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            logEvents.YouFinishCastingEvent += LogEvents_YouFinishCastingEvent;
            logEvents.LineEvent += LogEvents_LineEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            //the purpose of this function is to fire off spell cast events for spells that do not have a landed message.
            //For example, many charm spells dont let the user know that it has landed. 
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds > userCastingSpell.casttime + 1000)
                {
                    var deltaOffset = (int)(userCastingSpell.casttime - dt.TotalMilliseconds);
                    if (SelfSpellsThatDontEmitCompletionLogMesssages.Contains(userCastingSpell.name))
                    {
                        debugOutput.WriteLine($"Casting spell guess based on timer for {userCastingSpell.name}", OutputType.Spells);
                        baseSpellYouCastingHandler.Handle(userCastingSpell, EQSpells.SpaceYou, EQSpells.SpaceYou, deltaOffset, e.TimeStamp);
                    }
                    debugOutput.WriteLine($"Clearing spell because {dt.TotalMilliseconds}ms has elapsed to complete casting {userCastingSpell.casttime + 1000}ms for the spell {userCastingSpell.name}", OutputType.Spells);
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.FinishUserCastingSpell();
                    });
                }
            }
        }

        private void LogEvents_YouFinishCastingEvent(object sender, YouFinishCastingEvent e)
        {
            baseSpellYouCastingHandler.Handle(e.Spell, EQSpells.SpaceYou, e.TargetName, 0, e.TimeStamp);
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.FinishUserCastingSpell();
            });
        }
    }
}
