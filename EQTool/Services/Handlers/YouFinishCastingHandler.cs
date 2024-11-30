using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace EQTool.Services.Handlers
{
    public class YouFinishCastingHandler : BaseHandler
    {
        private readonly List<string> SelfSpellsThatDontEmitCompletionLogMesssages = new List<string>()
        {
            "Harmshield",
            "Divine Aura",
            "Dictate"
        };
        private readonly IAppDispatcher appDispatcher;
        private readonly BaseSpellYouCastingHandler baseSpellYouCastingHandler;
        public YouFinishCastingHandler(BaseSpellYouCastingHandler baseSpellYouCastingHandler, IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            this.appDispatcher = appDispatcher;
            this.logEvents.YouFinishCastingEvent += LogEvents_YouFinishCastingEvent;
            this.logEvents.LineEvent += LogEvents_LineEvent;
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
                    Debug.WriteLine($"Timer has expired on UserCastingSpell {userCastingSpell.name}");
                    var deltaOffset = (int)(userCastingSpell.casttime - dt.TotalMilliseconds);
                    if (SelfSpellsThatDontEmitCompletionLogMesssages.Contains(userCastingSpell.name))
                    {
                        baseSpellYouCastingHandler.Handle(userCastingSpell, EQSpells.SpaceYou, deltaOffset, e.TimeStamp);
                    }
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
