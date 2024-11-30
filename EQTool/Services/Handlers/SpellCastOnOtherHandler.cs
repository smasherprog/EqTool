using EQTool.Models;
using EQTool.ViewModels;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnOtherHandler : BaseHandler
    {
        private readonly IAppDispatcher appDispatcher;
        private readonly BaseSpellYouCastingHandler baseSpellYouCastingHandler;
        private readonly SpellDurations spellDurations;
        private readonly DebugOutput debugOutput;

        public SpellCastOnOtherHandler(
            SpellDurations spellDurations,
            LogEvents logEvents,
            BaseSpellYouCastingHandler baseSpellYouCastingHandler,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            IAppDispatcher appDispatcher,
            ITextToSpeach textToSpeach,
            DebugOutput debugOutput) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.debugOutput = debugOutput;
            this.appDispatcher = appDispatcher;
            this.spellDurations = spellDurations;
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            this.logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds >= (userCastingSpell.casttime - 300) && e.Spells.Any(a => a.name == userCastingSpell.name))
                {
                    debugOutput.WriteLine($"Casting spell guess based on timer for {userCastingSpell.name} on Target: {e.TargetName}", OutputType.Spells);
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.UserCastingSpell = null;
                        activePlayer.UserCastSpellDateTime = null;
                    });
                    baseSpellYouCastingHandler.Handle(userCastingSpell, e.TargetName, 0, e.TimeStamp);
                    return;
                }
                else
                {
                    userCastingSpell = null;
                    userCastSpellDateTime = null;
                }
            }

            if (userCastingSpell == null)
            {
                userCastingSpell = spellDurations.MatchClosestLevelToSpell(e.Spells, e.TimeStamp);
                if (userCastingSpell == null)
                {
                    debugOutput.WriteLine($"Could not match spell for '{e.Line}'", OutputType.Spells, MessageType.Error);
                }
            }

            baseSpellYouCastingHandler.Handle(userCastingSpell, e.TargetName, 0, e.TimeStamp);
        }
    }
}
