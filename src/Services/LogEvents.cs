using EQTool.Services.Parsing;
using System;
using static EQTool.Services.LogParser;

namespace EQTool.Services
{
    public class LogEvents
    {
        public event EventHandler<PlayerLocationEventArgs> PlayerLocationEvent;
        public void Handle(PlayerLocationEventArgs e)
        {
            PlayerLocationEvent?.Invoke(this, e);
        }

        public event EventHandler<CampEventArgs> CampEvent;
        public void Handle(CampEventArgs e)
        {
            CampEvent?.Invoke(this, e);
        }

        public event EventHandler<FTEParserData> FTEEvent;
        public void Handle(FTEParserData e)
        {
            FTEEvent?.Invoke(this, e);
        }

        public event EventHandler<FightHitEventArgs> FightHitEvent;
        public void Handle(FightHitEventArgs e)
        {
            FightHitEvent?.Invoke(this, e);
        }

        public event EventHandler<ConEventArgs> ConEvent;
        public void Handle(ConEventArgs e)
        {
            ConEvent?.Invoke(this, e);
        }

        public event EventHandler<DeadEventArgs> DeadEvent;
        public void Handle(DeadEventArgs e)
        {
            DeadEvent?.Invoke(this, e);
        }
    }
}
