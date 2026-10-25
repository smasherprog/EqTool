using EQTool.Models;
using System;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class TriggerHandler : BaseHandler
    {
        private readonly TriggerActionExecutor executor;
        private readonly TriggerTimerManager timerManager;

        public TriggerHandler(TriggerActionExecutor executor, TriggerTimerManager timerManager, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.executor = executor;
            this.timerManager = timerManager;
            logEvents.LineEvent += LogEvents_LineEvent;
            logEvents.SpellLandedEvent += LogEvents_SpellLandedEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            var currentZone = activePlayer?.Player?.Zone;
            var currentServer = activePlayer?.Player?.Server;
            var currentPlayerName = activePlayer?.Player?.Name ?? string.Empty;
            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled && !a.IsSpellTrigger).ToList())
            {
                try
                {
                    // skip triggers restricted to a zone the player isn't currently in
                    if (!trigger.MatchesZone(currentZone))
                    {
                        continue;
                    }

                    // skip triggers restricted to a server the player isn't on
                    if (!trigger.MatchesServer(currentServer))
                    {
                        continue;
                    }

                    // A {c} pattern with no known player name collapses to an empty string, so
                    // "{c} tells you" would become " tells you" and fire on anyone's tell (and a
                    // pattern of just "{c}" would match every line). Wait for the name instead.
                    if (trigger.UsesPlayerNameToken && string.IsNullOrEmpty(currentPlayerName))
                    {
                        continue;
                    }

                    trigger.PlayerName = currentPlayerName;
                    if (!trigger.Matches(e.Line))
                    {
                        continue;
                    }

                    FireLogLineTrigger(trigger);
                }
                catch (System.Exception ex)
                {
                    // One misbehaving trigger must not take the rest of the line with it. LineEvent
                    // is a multicast delegate, so an exception escaping here skips every handler
                    // subscribed after this one (deaths, spells, DPS) for this line - and in
                    // DEBUG/TEST builds LogParser.MainRun has no try/catch to stop it either.
                    App.LogUnhandledException(ex, $"TriggerHandler trigger '{trigger.TriggerName}'", activePlayer?.Player?.Server);
                }

                // A line is consumed by the first matching trigger; stop checking the rest.
                return;
            }
        }

        private void LogEvents_SpellLandedEvent(object sender, SpellLandedEvent e)
        {
            if (e.Spell == null)
            {
                return;
            }

            var currentZone = activePlayer?.Player?.Zone;
            var currentServer = activePlayer?.Player?.Server;
            var currentPlayerName = activePlayer?.Player?.Name ?? string.Empty;
            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled && a.IsSpellTrigger).ToList())
            {
                try
                {
                    if (!trigger.MatchesZone(currentZone) || !trigger.MatchesServer(currentServer))
                    {
                        continue;
                    }

                    trigger.PlayerName = currentPlayerName;
                    if (!trigger.MatchesSpell(e.Spell.name, e.TargetName))
                    {
                        continue;
                    }

                    FireSpellTrigger(trigger, e);
                }
                catch (System.Exception ex)
                {
                    App.LogUnhandledException(ex, $"TriggerHandler trigger '{trigger.TriggerName}'", activePlayer?.Player?.Server);
                }

                // Like log lines, a landing is consumed by the first matching trigger.
                return;
            }
        }

        // Raised for every trigger fired, so the editor's Test button can tell whether the live
        // loop fired the trigger it is testing.
        public event EventHandler<Trigger> TriggerFired;

        // Runs the outputs of a log-line trigger whose Matches just succeeded (setting its
        // placeholder values). Public so the editor's Test button can fire a trigger the live
        // loop skipped.
        public void FireLogLineTrigger(Trigger trigger)
        {
            FireBasic(trigger);

            // Timer and Counter tabs; the manager owns the enabled/configured checks
            timerManager.HandleTimerMatch(trigger);
            timerManager.HandleCounterMatch(trigger);
        }

        // Spell-trigger counterpart of FireLogLineTrigger, for a landing MatchesSpell accepted.
        public void FireSpellTrigger(Trigger trigger, SpellLandedEvent landed)
        {
            FireBasic(trigger);

            // Spell triggers have no Timer or Counter tab; Timer Ending / Timer Ended run
            // off the spell's own duration.
            timerManager.HandleSpellMatch(trigger, landed.Spell.name, landed.TargetName, landed.Duration);
        }

        private void FireBasic(Trigger trigger)
        {
            // bump the {COUNTER} tally before expanding any output so it reflects this match
            trigger.CurrentCounter++;

            executor.Execute(trigger.GetEffectiveBasic(), trigger.Expand);
            TriggerFired?.Invoke(this, trigger);
        }
    }
}
