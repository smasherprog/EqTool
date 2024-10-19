using EQTool.Models;
using System;
using System.Collections.Generic;

namespace EQTool.Services
{
    public class DeathLoopService
    {
        private readonly LogEvents logEvents;
        //private DateTime? lastActionTime;
        //private readonly List<DateTime> lastDeathTimes = new List<DateTime>();
        //private DateTime? lastHitTime;

        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public DeathLoopService(LogEvents logEvents, EQToolSettings eQToolSettings)
        {
            this.logEvents = logEvents;
            this.logEvents.DeadEvent += LogEvents_DeadEvent;
            this.logEvents.DamageEvent += LogEvents_DamageEvent;

            //this.logEvents.DamageEvent += LogEvents_FightHitEvent;
            //this.logEvents.SpellCastEvent += LogEvents_SpellCastEvent;
            //this.logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
            //this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
        }

        public bool IsDeathLooping { get; private set; } = false;

        //
        // function that gets called with a DeadEvent is received
        //
        private void LogEvents_DeadEvent(object sender, DeadEvent e)
        {
            // just a little audible marker
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
            player.Play();
        }

        //
        // function that gets called with a DeadEvent is received
        //
        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            // if the player is meleeing, then flush to death list
            if (e.AttackerName == "You")
            {
                // just a little audible marker
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();
            }
        }


        //private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        //{
        //    lastHitTime = lastActionTime = null;
        //    lastDeathTimes.Clear();
        //}

        //private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        //{
        //    lastActionTime = null;
        //}

        //private void LogEvents_SpellCastEvent(object sender, SpellCastEvent e)
        //{
        //    if (e.CastByYou)
        //    {
        //        lastActionTime = DateTime.Now;
        //    }
        //}

        //private void LogEvents_FightHitEvent(object sender, DamageEvent e)
        //{
        //    if (string.Equals(e.HitInformation.AttackerName, "You", StringComparison.OrdinalIgnoreCase))
        //    {
        //        lastActionTime = e.HitInformation.TimeStamp;
        //    }
        //    else if (string.Equals(e.HitInformation.AttackerName, "You", StringComparison.OrdinalIgnoreCase))
        //    {
        //        lastHitTime = e.HitInformation.TimeStamp;
        //    }
        //}

    }
}
