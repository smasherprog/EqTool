using EQTool.Models;
using System;

namespace EQTool.Services
{
    public class LogEvents
    {
        public event EventHandler<ExpGainedEvent> ExpGainedEvent;
        public void Handle(ExpGainedEvent e)
        {
            ExpGainedEvent?.Invoke(this, e);
        }

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

        public event EventHandler<ConEvent> ConEvent;
        public void Handle(ConEvent e)
        {
            ConEvent?.Invoke(this, e);
        }

        public event EventHandler<SlainEvent> SlainEvent;
        public void Handle(SlainEvent e)
        {
            SlainEvent?.Invoke(this, e);
        }

        public event EventHandler<ConfirmedDeathEvent> ConfirmedDeathEvent;
        public void Handle(ConfirmedDeathEvent e)
        {
            ConfirmedDeathEvent?.Invoke(this, e);
        }
        public event EventHandler<CommsEvent> CommsEvent;
        public void Handle(CommsEvent e)
        {
            CommsEvent?.Invoke(this, e);
        }

        public event EventHandler<PetEvent> PetEvent;
        public void Handle(PetEvent e)
        {
            PetEvent?.Invoke(this, e);
        }

        public event EventHandler<LoadingPleaseWaitEvent> LoadingPleaseWaitEvent;
        public void Handle(LoadingPleaseWaitEvent e)
        {
            LoadingPleaseWaitEvent?.Invoke(this, e);
        }

        public event EventHandler<WelcomeEvent> WelcomeEvent;
        public void Handle(WelcomeEvent e)
        {
            WelcomeEvent?.Invoke(this, e);
        }

        public event EventHandler<GroupLeaderEvent> GroupLeaderEvent;
        public void Handle(GroupLeaderEvent e)
        {
            GroupLeaderEvent?.Invoke(this, e);
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

        public event EventHandler<YourItemBeginsToGlow> YourItemBeginsToGlow;
        public void Handle(YourItemBeginsToGlow e)
        {
            YourItemBeginsToGlow?.Invoke(this, e);
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

        public event EventHandler<LineEvent> LineEvent;
        public void Handle(LineEvent e)
        {
            LineEvent?.Invoke(this, e);
        }

        public event EventHandler<OverlayEvent> OverlayEvent;
        public void Handle(OverlayEvent e)
        {
            OverlayEvent?.Invoke(this, e);
        }

        public event EventHandler<YourSpellInterupptedEvent> YourSpellInterupptedEvent;
        public void Handle(YourSpellInterupptedEvent e)
        {
            YourSpellInterupptedEvent?.Invoke(this, e);
        }

        public event EventHandler<YouBeginCastingEvent> YouBeginCastingEvent;
        public void Handle(YouBeginCastingEvent e)
        {
            YouBeginCastingEvent?.Invoke(this, e);
        }

        public event EventHandler<YouFinishCastingEvent> YouFinishCastingEvent;
        public void Handle(YouFinishCastingEvent e)
        {
            YouFinishCastingEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellCastOnYouEvent> SpellCastOnYouEvent;
        public void Handle(SpellCastOnYouEvent e)
        {
            SpellCastOnYouEvent?.Invoke(this, e);
        }

        public event EventHandler<SpellCastOnOtherEvent> SpellCastOnOtherEvent;
        public void Handle(SpellCastOnOtherEvent e)
        {
            SpellCastOnOtherEvent?.Invoke(this, e);
        }

        public event EventHandler<MendWoundsEvent> MendWoundsEvent;
        public void Handle(MendWoundsEvent e)
        {
            MendWoundsEvent?.Invoke(this, e);
        }

        public event EventHandler<DragonRoarEvent> DragonRoarEvent;
        public void Handle(DragonRoarEvent e)
        {
            DragonRoarEvent?.Invoke(this, e);
        }

        public event EventHandler<DragonRoarRemoteEvent> DragonRoarRemoteEvent;
        public void Handle(DragonRoarRemoteEvent e)
        {
            DragonRoarRemoteEvent?.Invoke(this, e);
        }
        public event EventHandler<YouHaveFinishedMemorizingEvent> YouHaveFinishedMemorizingEvent;
        public void Handle(YouHaveFinishedMemorizingEvent e)
        {
            YouHaveFinishedMemorizingEvent?.Invoke(this, e);
        }

        public event EventHandler<PlayerDisconnectReceivedRemoteEvent> PlayerDisconnectReceivedRemoteEvent;
        public void Handle(PlayerDisconnectReceivedRemoteEvent e)
        {
            PlayerDisconnectReceivedRemoteEvent?.Invoke(this, e);
        }

        public event EventHandler<OtherPlayerLocationReceivedRemoteEvent> OtherPlayerLocationReceivedRemoteEvent;
        public void Handle(OtherPlayerLocationReceivedRemoteEvent e)
        {
            OtherPlayerLocationReceivedRemoteEvent?.Invoke(this, e);
        }

        public event EventHandler<NPCBeginsToGateEvent> NPCBeginsToGateEvent;
        public void Handle(NPCBeginsToGateEvent e)
        {
            NPCBeginsToGateEvent?.Invoke(this, e);
        }
        public event EventHandler<RingWarEvent> RingWarEvent;
        public void Handle(RingWarEvent e)
        {
            RingWarEvent?.Invoke(this, e);
        }

        public event EventHandler<BoatEvent> BoatEvent;
        public void Handle(BoatEvent e)
        {
            BoatEvent?.Invoke(this, e);
        } 
    }
}
