using EQTool.Models;
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
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            var currentZone = activePlayer?.Player?.Zone;
            var currentServer = activePlayer?.Player?.Server;
            var currentPlayerName = activePlayer?.Player?.Name ?? string.Empty;
            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled).ToList())
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

                    // bump the {COUNTER} tally before expanding any output so it reflects this match
                    trigger.CurrentCounter++;

                    // Basic tab output (display text / audio)
                    executor.Execute(trigger.GetEffectiveBasic(), trigger.Expand);

                    // Timer and Counter tabs; the manager owns the enabled/configured checks
                    timerManager.HandleTimerMatch(trigger);
                    timerManager.HandleCounterMatch(trigger);
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
    }
}
