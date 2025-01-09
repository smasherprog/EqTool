using EQTool.Models;
using System.Collections.Generic;

namespace EQTool.Services.Handlers
{
    public class YouFinishCastingHandler : BaseHandler
    {
        private readonly List<string> SelfSpellsThatDontEmitCompletionLogMesssages = new List<string>()
        {
            "Harmshield",
            "Divine Aura",
            "Dictate",
            "Charm",
            "Beguile",
            "Cajoling Whispers",
            "Allure",
            "Boltran's Agacerie"
        };
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
                        baseSpellYouCastingHandler.Handle(userCastingSpell, EQSpells.SpaceYou, deltaOffset, e.TimeStamp);
                    }
                    debugOutput.WriteLine($"Clearing spell because {dt.TotalMilliseconds}ms has elapsed to complete casting {userCastingSpell.casttime + 1000}ms for the spell {userCastingSpell.name}", OutputType.Spells);
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.UserCastingSpell = null;
                        activePlayer.UserCastSpellDateTime = null;
                    });
                }

            }
        }

        private void LogEvents_YouFinishCastingEvent(object sender, YouFinishCastingEvent e)
        {
            baseSpellYouCastingHandler.Handle(e.Spell, e.TargetName, 0, e.TimeStamp);
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = null;
                activePlayer.UserCastSpellDateTime = null;
            });
        }
    }
}
