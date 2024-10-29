using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class SlainHandler : BaseHandler
    {
        private string Victim = string.Empty;
        private string Killer = string.Empty;
        private int LineNumber = -100;//just some value that can never exist
        private bool AlreadyEmitted = false;

        public SlainHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
            this.logEvents.ExperienceGainedEvent += LogEvents_ExperienceGainedEvent;
            this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
            this.logEvents.LineEvent += LogEvents_LineEvent; ;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            if (!string.IsNullOrWhiteSpace(Victim))
            {
                logEvents.Handle(new NewSlainEvent
                {
                    Killer = Killer,
                    Line = e.Line,
                    LineCounter = e.LineCounter,
                    TimeStamp = e.TimeStamp,
                });
            }
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            LineNumber = -100;
            Victim = string.Empty;
            AlreadyEmitted = false;
        }

        private void LogEvents_ExperienceGainedEvent(object sender, ExperienceGainedEvent e)
        {
            if (e.LineCounter - 1 == LineNumber)
            {
                if (AlreadyEmitted)
                {
                    LineNumber = e.LineCounter;
                }
            }
            else if (!AlreadyEmitted && !string.IsNullOrWhiteSpace(Victim))
            {

            }
        }

        private void LogEvents_FactionEvent(object sender, FactionEvent e)
        {


        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {
            Victim = e.Victim;
            Killer = e.Killer;
            LineNumber = e.LineCounter;
            logEvents.Handle(new NewSlainEvent
            {
                Killer = Killer,
                Line = e.Line,
                LineCounter = e.LineCounter,
                TimeStamp = e.TimeStamp,
            });
            AlreadyEmitted = true;
        }
    }
}
