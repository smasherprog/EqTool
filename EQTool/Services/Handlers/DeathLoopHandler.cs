using EQTool.Models;
using System;
using System.Diagnostics;

namespace EQTool.Services.Handlers
{
    public class DeathLoopHandler : BaseHandler
    {
        // todo - make these values configurable
        private readonly int _deathLoopDeaths = 4;
        private readonly int _deathLoopSeconds = 120;

        // Oldest death at position 0, newest appended. Entries roll off once they fall outside
        // _deathLoopSeconds, and the whole list is flushed on any sign the player is not AFK
        // (casting, meleeing, or communicating), since a death loop only counts unattended deaths.
        private readonly System.Collections.Generic.List<DateTime> _deathLoopTimeStamps = new System.Collections.Generic.List<DateTime>();

        public DeathLoopHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.SlainEvent += LogEvents_DeathEvent;
            logEvents.DamageEvent += LogEvents_DamageEvent;
            logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
            logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        public bool IsDeathLooping()
        {
            return _deathLoopTimeStamps.Count >= _deathLoopDeaths;
        }

        public int DeathCount()
        {
            return _deathLoopTimeStamps.Count;
        }


        private void WriteDeathTimes()
        {
            Debug.WriteLine($"Death timestamps: count = {_deathLoopTimeStamps.Count}, times = ");
            for (var i = 0; i < _deathLoopTimeStamps.Count; i++)
            {
                Debug.Write($"[{_deathLoopTimeStamps[i]}] ");
            }

            Debug.WriteLine(string.Empty);
        }

        private void LogEvents_DeathEvent(object sender, SlainEvent deathEvent)
        {
            UpdateDeathList(deathEvent.TimeStamp);

            if (deathEvent.Victim == "You")
            {
                _deathLoopTimeStamps.Add(deathEvent.TimeStamp);
                WriteDeathTimes();

                if (_deathLoopTimeStamps.Count >= _deathLoopDeaths)
                {
                    DeathLoopResponse(deathEvent.TimeStamp, deathEvent.Line);
                }
            }
        }

        private void UpdateDeathList(DateTime currentTimeStamp)
        {
            if (_deathLoopTimeStamps.Count > 0)
            {
                var done = false;
                while (!done)
                {
                    if (_deathLoopTimeStamps.Count == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        var oldestTimestamp = _deathLoopTimeStamps[0];
                        var elapsedSeconds = (currentTimeStamp - oldestTimestamp).TotalSeconds;

                        if (elapsedSeconds > _deathLoopSeconds)
                        {
                            _deathLoopTimeStamps.RemoveAt(0);
                            WriteDeathTimes();
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
            }
        }

        public void DeathLoopResponse(DateTime timestamp, string line)
        {
            // we cannot kill eqgame.exe, so the only recourse is to yell at the player
            if (activePlayer?.Player?.DeathLoopAudio == true)
            {
                textToSpeach.Say("death loop death loop death loop. death loop!");
            }

            Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
            Console.WriteLine($"{_deathLoopDeaths} or more deaths in less than {_deathLoopSeconds} seconds, with no player activity");
            WriteDeathTimes();
            Console.WriteLine("We really should be killing the eqgame.exe process right now");
            Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
        }

        private void LogEvents_DamageEvent(object sender, DamageEvent damageEvent)
        {
            UpdateDeathList(damageEvent.TimeStamp);

            if (damageEvent.AttackerName == "You")
            {
                _deathLoopTimeStamps.Clear();
            }
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent youBeginCastingEvent)
        {
            _deathLoopTimeStamps.Clear();
        }


        private void LogEvents_CommsEvent(object sender, CommsEvent commsEvent)
        {
            UpdateDeathList(commsEvent.TimeStamp);

            if ((commsEvent.TheChannel != CommsEvent.Channel.NONE) && (commsEvent.Sender == "You"))
            {
                _deathLoopTimeStamps.Clear();
            }
        }
    }
}
