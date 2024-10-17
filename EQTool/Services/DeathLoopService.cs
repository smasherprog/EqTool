using EQTool.Models;
using System;
using System.Collections.Generic;

namespace EQTool.Services
{
    public class DeathLoopService
    {
        private readonly LogEvents logEvents;
        private DateTime? lastActionTime;
        private readonly List<DateTime> lastDeathTimes = new List<DateTime>();
        private DateTime? lastHitTime;

        public DeathLoopService(LogEvents logEvents, EQToolSettings eQToolSettings)
        {
            this.logEvents = logEvents;
            this.logEvents.DeadEvent += LogEvents_DeadEvent;
            this.logEvents.FightHitEvent += LogEvents_FightHitEvent;
            this.logEvents.SpellCastEvent += LogEvents_SpellCastEvent;
            this.logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
            this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent; ;
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            lastHitTime = lastActionTime = null;
            lastDeathTimes.Clear();
        }

        private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        {
            lastActionTime = null;
        }

        private void LogEvents_SpellCastEvent(object sender, SpellCastEvent e)
        {
            if (e.CastByYou)
            {
                lastActionTime = DateTime.Now;
            }
        }

        private void LogEvents_FightHitEvent(object sender, FightHitEvent e)
        {
            if (string.Equals(e.HitInformation.SourceName, "You", StringComparison.OrdinalIgnoreCase))
            {
                lastActionTime = e.HitInformation.TimeStamp;
            }
            else if (string.Equals(e.HitInformation.SourceName, "You", StringComparison.OrdinalIgnoreCase))
            {
                lastHitTime = e.HitInformation.TimeStamp;
            }
        }

        private void LogEvents_DeadEvent(object sender, DeadEvent e)
        {
            lastDeathTimes.Add(DateTime.Now);
        }
    }
}
