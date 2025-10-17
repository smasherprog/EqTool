using EQTool.Models;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnOtherHandler : BaseHandler
    {
        private readonly SpellHandlerService baseSpellYouCastingHandler;
        private readonly SpellDurations spellDurations;

        public SpellCastOnOtherHandler(
            SpellDurations spellDurations,
            SpellHandlerService baseSpellYouCastingHandler, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellDurations = spellDurations;
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds >= (userCastingSpell.casttime - 600) && e.Spells.Any(a => a.name == userCastingSpell.name))
                {
                    debugOutput.WriteLine($"Casting spell guess based on timer for {userCastingSpell.name} on Target: {e.TargetName}", OutputType.Spells);
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.UserCastingSpell = null;
                        activePlayer.UserCastSpellDateTime = null;
                    });
                    var target = e.TargetName;
                    if (string.Equals(userCastingSpell.name, "Theft of Thought", System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(userCastingSpell.name, "dictate", System.StringComparison.OrdinalIgnoreCase)
                        )
                    {
                        target = EQSpells.SpaceYou;
                    }
                    baseSpellYouCastingHandler.Handle(userCastingSpell, EQSpells.SpaceYou, target, 0, e.TimeStamp);
                    return;
                }
                else
                {
                    debugOutput.WriteLine($"Skipped dt >= {userCastingSpell.casttime - 600} AND {e.Spells.Any(a => a.name == userCastingSpell.name)}", OutputType.Spells);
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

            baseSpellYouCastingHandler.Handle(userCastingSpell, userCastingSpell?.NameIfSelfCast(e.TargetName), e.TargetName, 0, e.TimeStamp);
        }
    }
}
