using EQTool.Models;
using System.Linq;
using EQToolShared.Enums;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnOtherHandler : BaseHandler
    {
        private readonly SpellHandlerService baseSpellYouCastingHandler;
        private readonly CasterGuessingService casterGuessingService;
        private readonly SpellDurations spellDurations;

        public SpellCastOnOtherHandler(
            SpellHandlerService baseSpellYouCastingHandler,
            CasterGuessingService casterGuessingService,
            SpellDurations spellDurations,
            BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.baseSpellYouCastingHandler = baseSpellYouCastingHandler;
            this.casterGuessingService = casterGuessingService;
            this.spellDurations = spellDurations;
            logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var isValidCast = false;
                var target = e.TargetName;
                var dt = e.TimeStamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds >= (userCastingSpell.casttime - 600) && e.Spells.Any(a => a.name == userCastingSpell.name))
                {
                    debugOutput.WriteLine($"Casting spell guess based on timer for {userCastingSpell.name} on Target: {e.TargetName}", OutputType.Spells);
                    isValidCast = true;
                    if (EQSpells.Charms.Any(charmSpell => string.Equals(userCastingSpell.name, charmSpell, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        target = EQSpells.SpaceYou;
                    }
                }
                else if (activePlayer.Player.PlayerClass == PlayerClasses.Enchanter && userCastingSpell.name == "Alliance" && dt.TotalMilliseconds <= 0.2)
                {
                    debugOutput.WriteLine($"Handling Rod of Insidious Glamour Clicky ({userCastingSpell.name}) on Target: {e.TargetName}", OutputType.Spells);
                    isValidCast = true;
                }
                else
                {
                    debugOutput.WriteLine($"Dubious player cast detected: dt >= {userCastingSpell.casttime - 600} for spell {e.Spells.Any(a => a.name == userCastingSpell.name)}", OutputType.Spells);
                    userCastingSpell = null;
                    userCastSpellDateTime = null;
                    isValidCast = false;
                }

                if (isValidCast)
                {
                    activePlayer.FinishUserCastingSpell();
                    baseSpellYouCastingHandler.Handle(userCastingSpell, EQSpells.SpaceYou, target, 0, e.TimeStamp);
                    return;
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

            var caster = casterGuessingService.TryGuessNameForTimer(userCastingSpell, e.TargetName, requireCertainty: false);
            baseSpellYouCastingHandler.Handle(userCastingSpell, caster, e.TargetName, 0, e.TimeStamp);
        }
    }
}
