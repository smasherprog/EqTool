using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class YouBeginCastingHandler : BaseHandler
    {
        private readonly IAppDispatcher appDispatcher;

        public YouBeginCastingHandler(IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
            this.logEvents.YourSpellInterupptedEvent += LogEvents_YourSpellInterupptedEvent;
            this.logEvents.LineEvent += LogEvents_LineEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalSeconds > userCastingSpell.casttime * 2)
                { 
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.UserCastingSpell = null;
                        activePlayer.UserCastSpellDateTime = null;
                    });
                }

            }
        }

        private void LogEvents_YourSpellInterupptedEvent(object sender, YourSpellInterupptedEvent e)
        { 
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = null;
                activePlayer.UserCastSpellDateTime = null;
            });
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        { 
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = e.Spell;
                activePlayer.UserCastSpellDateTime = e.TimeStamp;
            });
        }
    }
}
