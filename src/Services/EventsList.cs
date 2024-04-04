using EQTool.Models;
using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using static EQTool.Services.Parsing.ChParser;

namespace EQTool.Services
{
    public class EventsList
    {
        public class PlayerZonedEventArgs : EventArgs
        {
            public string Zone { get; set; }
        }
        public class PlayerLocationEventArgs : EventArgs
        {
            public Point3D Location { get; set; }
            public PlayerInfo PlayerInfo { get; set; }
        }
        public class FightHitEventArgs : EventArgs
        {
            public DPSParseMatch HitInformation { get; set; }
        }
        public class DeadEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class ConEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class StartTimerEventArgs : EventArgs
        {
            public CustomTimer CustomTimer { get; set; }
        }

        public class CancelTimerEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class SpellEventArgs : EventArgs
        {
            public SpellParsingMatch Spell { get; set; }
        }

        public class SpellWornOffOtherEventArgs : EventArgs
        {
            public string SpellName { get; set; }
        }

        public class SpellWornOffSelfEventArgs : EventArgs
        {
            public List<string> SpellNames { get; set; }
        }

        public class WhoPlayerEventArgs : EventArgs
        {
            public EQToolShared.APIModels.PlayerControllerModels.Player PlayerInfo { get; set; }
        }
        public class RandomRollEventArgs : EventArgs
        {
            public RandomParser.RandomRollData RandomRollData { get; set; }
        }

        public class WhoEventArgs : EventArgs { }
        public class CampEventArgs : EventArgs { }
        public class EnteredWorldArgs : EventArgs { }
        public class QuakeArgs : EventArgs { }
        public class CharmBreakArgs : EventArgs { }

        public event EventHandler<RandomRollEventArgs> RandomRollEvent;
        public void Handle(RandomRollEventArgs e) { RandomRollEvent?.Invoke(this, e); }
        public event EventHandler<WhoEventArgs> WhoEvent;
        public void Handle(WhoEventArgs e) { WhoEvent?.Invoke(this, e); }
        public event EventHandler<WhoPlayerEventArgs> WhoPlayerEvent;
        public void Handle(WhoPlayerEventArgs e) { WhoPlayerEvent?.Invoke(this, e); }
        public event EventHandler<SpellWornOffSelfEventArgs> SpellWornOffSelfEvent;
        public void Handle(SpellWornOffSelfEventArgs e) { SpellWornOffSelfEvent?.Invoke(this, e); }
        public event EventHandler<QuakeArgs> QuakeEvent;
        public void Handle(QuakeArgs e) { QuakeEvent?.Invoke(this, e); }
        public event EventHandler<POFDTParser.POF_DT_Event> POFDTEvent;
        public void Handle(POFDTParser.POF_DT_Event e) { POFDTEvent?.Invoke(this, e); }
        public event EventHandler<EnrageParser.EnrageEvent> EnrageEvent;
        public void Handle(EnrageParser.EnrageEvent e) { EnrageEvent?.Invoke(this, e); }
        public event EventHandler<ChParseData> CHEvent;
        public void Handle(ChParseData e) { CHEvent?.Invoke(this, e); }
        public event EventHandler<LevParser.LevStatus> LevEvent;
        public void Handle(LevParser.LevStatus e) { LevEvent?.Invoke(this, e); }
        public event EventHandler<InvisParser.InvisStatus> InvisEvent;
        public void Handle(InvisParser.InvisStatus e) { InvisEvent?.Invoke(this, e); }
        public event EventHandler<FTEParser.FTEParserData> FTEEvent;
        public void Handle(FTEParser.FTEParserData e) { FTEEvent?.Invoke(this, e); }
        public event EventHandler<CharmBreakArgs> CharmBreakEvent;
        public void Handle(CharmBreakArgs e) { CharmBreakEvent?.Invoke(this, e); }
        public event EventHandler<string> FailedFeignEvent;
        public void HandleFailedFeign(string e) { FailedFeignEvent?.Invoke(this, e); }
        public event EventHandler<string> GroupInviteEvent;
        public void HandleGroupInvite(string e) { GroupInviteEvent?.Invoke(this, e); }
        public event EventHandler<SpellWornOffOtherEventArgs> SpellWornOtherOffEvent;
        public void Handle(SpellWornOffOtherEventArgs e) { SpellWornOtherOffEvent?.Invoke(this, e); }
        public event EventHandler<ResistSpellParser.ResistSpellData> ResistSpellEvent;
        public void Handle(ResistSpellParser.ResistSpellData e) { ResistSpellEvent?.Invoke(this, e); }
        public event EventHandler<SpellEventArgs> StartCastingEvent;
        public void Handle(SpellEventArgs e) { StartCastingEvent?.Invoke(this, e); }
        public event EventHandler<CancelTimerEventArgs> CancelTimerEvent;
        public void Handle(CancelTimerEventArgs e) { CancelTimerEvent?.Invoke(this, e); }
        public event EventHandler<StartTimerEventArgs> StartTimerEvent;
        public void Handle(StartTimerEventArgs e) { StartTimerEvent?.Invoke(this, e); }
        public event EventHandler<ConEventArgs> ConEvent;
        public void Handle(ConEventArgs e) { ConEvent?.Invoke(this, e); }
        public event EventHandler<DeadEventArgs> DeadEvent;
        public void Handle(DeadEventArgs e) { DeadEvent?.Invoke(this, e); }
        public event EventHandler<FightHitEventArgs> FightHitEvent;
        public void Handle(FightHitEventArgs e) { FightHitEvent?.Invoke(this, e); }
        public event EventHandler<PlayerZonedEventArgs> PlayerZonedEvent;
        public void Handle(PlayerZonedEventArgs e) { PlayerZonedEvent?.Invoke(this, e); }
        public event EventHandler<PlayerLocationEventArgs> PlayerLocationEvent;
        public void Handle(PlayerLocationEventArgs e) { PlayerLocationEvent?.Invoke(this, e); }
        public event EventHandler<CampEventArgs> CampEvent;
        public void Handle(CampEventArgs e) { CampEvent?.Invoke(this, e); }
        public event EventHandler<EnteredWorldArgs> EnteredWorldEvent;
        public void Handle(EnteredWorldArgs e) { EnteredWorldEvent?.Invoke(this, e); }
    }
}
