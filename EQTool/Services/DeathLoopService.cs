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
            this.logEvents.SpellCastEvent += LogEvents_SpellCastEvent;

        }

        public bool IsDeathLooping { get; private set; } = false;

        //
        // function that gets called for this particular Event type
        //
        private void LogEvents_DeadEvent(object sender, DeadEvent e)
        {
            // todo - remove for final
            // just a little audible marker
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
            player.Play();
        }

        //
        // function that gets called for this particular Event type
        //
        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            // todo - remove for final
            // if the player is meleeing, then flush to death list
            if (e.AttackerName == "You")
            {
                // just a little audible marker
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();
            }
        }

        //
        // function that gets called for this particular Event type
        //
        private void LogEvents_SpellCastEvent(object sender, SpellCastEvent e)
        {
            if (e.CastByYou)
            {
                // todo - remove for final
                // just a little audible marker
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();
            }
        }
    }
}
