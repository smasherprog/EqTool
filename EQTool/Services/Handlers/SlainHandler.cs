using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class SlainHandler : BaseHandler
    {
        private string Victim = string.Empty;
        private string Killer = string.Empty;
        private readonly List<string> FactionMessages = new List<string>();
        private bool ExpMessage = false;
        private int LineNumber = -100;//just some value that can never exist
        private bool AlreadyEmitted = false;

        public SlainHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
#if DEBUG || TEST
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
            this.logEvents.ExpGainedEvent += LogEvents_ExperienceGainedEvent;
            this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
            this.logEvents.LineEvent += LogEvents_LineEvent;
#endif
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            EmitSlainEvent(e);
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            Reset();
        }

        private void Reset()
        {
            LineNumber = -100;
            Killer = Victim = string.Empty;
            FactionMessages.Clear();
            ExpMessage = AlreadyEmitted = false;
        }

        private void LogEvents_ExperienceGainedEvent(object sender, ExpGainedEvent e)
        {
            Victim = "Experience Slain Guess";
            Killer = "You";
            if (this.ExpMessage)
            {
                logEvents.Handle(new NewSlainEvent
                {
                    Killer = Killer,
                    Victim = Victim,
                    Line = e.Line,
                    LineCounter = e.LineCounter,
                    TimeStamp = e.TimeStamp,
                });
                Reset();
                this.AlreadyEmitted = true;
            }
            Victim = "Experience Slain Guess";
            Killer = "You";
            LineNumber = e.LineCounter;
            ExpMessage = true;
        }

        private void LogEvents_FactionEvent(object sender, FactionEvent e)
        {
            LineNumber = e.LineCounter;
            Victim = "Faction Slain Guess";
            Killer = "You";

            if (FactionMessages.Contains(e.Line))
            {
                logEvents.Handle(new NewSlainEvent
                {
                    Killer = Killer,
                    Victim = Victim,
                    Line = e.Line,
                    LineCounter = e.LineCounter,
                    TimeStamp = e.TimeStamp,
                });
                Reset();
            }
            else
            {
                FactionMessages.Add(e.Line);
            }
        }

        private void EmitSlainEvent(BaseLogParseEvent e)
        {
            if (e.LineCounter == LineNumber)
            {
                return;
            }
            else if (e.LineCounter - 1 == LineNumber)
            {
                if (AlreadyEmitted)
                {
                    LineNumber = e.LineCounter;
                }
                else if (this.ExpMessage || this.FactionMessages.Any())
                {
                    logEvents.Handle(new NewSlainEvent
                    {
                        Killer = Killer,
                        Victim = Victim,
                        Line = e.Line,
                        LineCounter = e.LineCounter,
                        TimeStamp = e.TimeStamp,
                    });
                    Reset();
                }
            }
        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {
            Victim = e.Victim;
            Killer = e.Killer;
            LineNumber = e.LineCounter;
            logEvents.Handle(new NewSlainEvent
            {
                Killer = Killer,
                Victim = Victim,
                Line = e.Line,
                LineCounter = e.LineCounter,
                TimeStamp = e.TimeStamp,
            });
            AlreadyEmitted = true;
        }
    }
}
