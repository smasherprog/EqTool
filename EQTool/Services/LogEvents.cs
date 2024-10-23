using EQTool.Models;
using System;

namespace EQTool.Services
{
    public class LogEvents
    {
        public event EventHandler<PlayerLocationEvent> PlayerLocationEvent;
        public void Handle(PlayerLocationEvent e)
        {
            PlayerLocationEvent?.Invoke(this, e);
        }

        public event EventHandler<CampEvent> CampEvent;
        public void Handle(CampEvent e)
        {
            CampEvent?.Invoke(this, e);
        }

        public event EventHandler<FTEEvent> FTEEvent;
        public void Handle(FTEEvent e)
        {
            FTEEvent?.Invoke(this, e);
        }

        public event EventHandler<DamageEvent> DamageEvent;
        public void Handle(DamageEvent e)
        {
            DamageEvent?.Invoke(this, e);
        }

        public event EventHandler<YouBeginCastingEvent> YouBeginCastingEvent;
        public void Handle(YouBeginCastingEvent e)
        {
            YouBeginCastingEvent?.Invoke(this, e);
        }

        public event EventHandler<ConEvent> ConEvent;
        public void Handle(ConEvent e)
        {
            ConEvent?.Invoke(this, e);
        }

        public event EventHandler<DeathEvent> DeathEvent;
        public void Handle(DeathEvent e)
        {
            DeathEvent?.Invoke(this, e);
        }

        public event EventHandler<CommsEvent> CommsEvent;
        public void Handle(CommsEvent e)
        {
            CommsEvent?.Invoke(this, e);
        }

        public event EventHandler<StartTimerEvent> StartTimerEvent;
        public void Handle(StartTimerEvent e)
        {
            StartTimerEvent?.Invoke(this, e);
        }

        public event EventHandler<CancelTimerEvent> CancelTimerEvent;
        public void Handle(CancelTimerEvent e)
        {
            CancelTimerEvent?.Invoke(this, e);
        }

        public event EventHandler<CharmBreakEvent> CharmBreakEvent;
        public void Handle(CharmBreakEvent e)
        {
            CharmBreakEvent?.Invoke(this, e);
        }

        public event EventHandler<QuakeEvent> QuakeEvent;
        public void Handle(QuakeEvent e)
        {
            QuakeEvent?.Invoke(this, e);
        }

        public event EventHandler<WhoPlayerEvent> WhoPlayerEvent;
        public void Handle(WhoPlayerEvent e)
        {
            WhoPlayerEvent?.Invoke(this, e);
        }

        public event EventHandler<WhoEvent> WhoEvent;
        public void Handle(WhoEvent e)
        {
            WhoEvent?.Invoke(this, e);
        }

        public event EventHandler<EnteredWorldEvent> EnteredWorldEvent;
        public void Handle(EnteredWorldEvent e)
        {
            EnteredWorldEvent?.Invoke(this, e);
        }

        public event EventHandler<EnrageEvent> EnrageEvent;
        public void Handle(EnrageEvent e)
        {
            EnrageEvent?.Invoke(this, e);
        }

        public event EventHandler<LevitateEvent> LevitateEvent;
        public void Handle(LevitateEvent e)
        {
            LevitateEvent?.Invoke(this, e);
        }

        public event EventHandler<InvisEvent> InvisEvent;
        public void Handle(InvisEvent e)
        {
            InvisEvent?.Invoke(this, e);
        }

        public event EventHandler<FailedFeignEvent> FailedFeignEvent;
        public void Handle(FailedFeignEvent e)
        {
            FailedFeignEvent?.Invoke(this, e);
        }

        public event EventHandler<GroupInviteEvent> GroupInviteEvent;
        public void Handle(GroupInviteEvent e)
        {
            GroupInviteEvent?.Invoke(this, e);
        }

        public event EventHandler<ResistSpellEvent> ResistSpellEvent;
        public void Handle(ResistSpellEvent e)
        {
            ResistSpellEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellWornOffEvent> SpellWornOffEvent;
        public void Handle(SpellWornOffEvent e)
        {
            SpellWornOffEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellWornOffSelfEvent> SpellWornOffSelfEvent;
        public void Handle(SpellWornOffSelfEvent e)
        {
            SpellWornOffSelfEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellWornOffOtherEvent> SpellWornOffOtherEvent;
        public void Handle(SpellWornOffOtherEvent e)
        {
            SpellWornOffOtherEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellCastEvent> SpellCastEvent;
        public void Handle(SpellCastEvent e)
        {
            SpellCastEvent?.Invoke(this, e);
        }

        public event EventHandler<YouZonedEvent> YouZonedEvent;
        public void Handle(YouZonedEvent e)
        {
            YouZonedEvent?.Invoke(this, e);
        }

        public event EventHandler<CompleteHealEvent> CompleteHealEvent;
        public void Handle(CompleteHealEvent e)
        {
            CompleteHealEvent?.Invoke(this, e);
        }

        public event EventHandler<DeathTouchEvent> DeathTouchEvent;
        public void Handle(DeathTouchEvent e)
        {
            DeathTouchEvent?.Invoke(this, e);
        }

        public event EventHandler<RandomRollEvent> RandomRollEvent;
        public void Handle(RandomRollEvent e)
        {
            RandomRollEvent?.Invoke(this, e);
        }

        public event EventHandler<PayerChangedEvent> PayerChangedEvent;
        public void Handle(PayerChangedEvent e)
        {
            PayerChangedEvent?.Invoke(this, e);
        }

        public event EventHandler<ClassDetectedEvent> ClassDetectedEvent;
        public void Handle(ClassDetectedEvent e)
        {
            ClassDetectedEvent?.Invoke(this, e);
        }

        public event EventHandler<PlayerLevelDetectionEvent> PlayerLevelDetectionEvent;
        public void Handle(PlayerLevelDetectionEvent e)
        {
            PlayerLevelDetectionEvent?.Invoke(this, e);
        }

        public event EventHandler<FactionEvent> FactionEvent;
        public void Handle(FactionEvent e)
        {
            FactionEvent?.Invoke(this, e);
        }

        public event EventHandler<SlainEvent> SlainEvent;
        public void Handle(SlainEvent e)
        {
            SlainEvent?.Invoke(this, e);
        }
    }
}
