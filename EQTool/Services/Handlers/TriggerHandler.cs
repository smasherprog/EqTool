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
            // give active timers a chance to end early on this line
            timerManager.OnLine(e.Line);

            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled).ToList())
            {
                trigger.PlayerName = activePlayer?.Player?.Name ?? string.Empty;
                if (!trigger.Matches(e.Line))
                {
                    continue;
                }

                // bump the {COUNTER} tally before expanding any output so it reflects this match
                trigger.CurrentCounter++;

                // Basic tab output (display text / clipboard / audio)
                executor.Execute(trigger.GetEffectiveBasic(), trigger.Expand);

                // Timer tab
                if (trigger.Timer != null && trigger.Timer.IsEnabled)
                {
                    timerManager.HandleTimerMatch(trigger);
                }

                // Counter tab
                if (trigger.Counter != null && trigger.Counter.ResetEnabled)
                {
                    timerManager.HandleCounterMatch(trigger);
                }

                // A line is consumed by the first matching trigger; stop checking the rest.
                return;
            }
        }
    }
}
