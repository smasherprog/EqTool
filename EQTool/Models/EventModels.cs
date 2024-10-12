using EQToolShared.HubModels;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using static EQTool.Services.Parsing.InvisParser;
using static EQTool.Services.Parsing.LevParser;

namespace EQTool.Models
{
    public class PlayerLocationEvent
    {
        public Point3D Location { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
    }
    public class WhoEvent { }
    public class CampEvent { }
    public class EnteredWorldEvent { }
    public class QuakeEvent { }
    public class CharmBreakEvent { }
    public class InvisEvent
    {
        public InvisStatus InvisStatus { get; set; }
    }
    public class LevitateEvent
    {
        public LevStatus LevitateStatus { get; set; }
    }
    public class FTEEvent
    {
        public string NPCName { get; set; }
        public string FTEPerson { get; set; }
    }
    public class EnrageEvent
    {
        public string NpcName { get; set; }
    }

    public class FightHitEvent
    {
        public DPSParseMatch HitInformation { get; set; }
    }

    public class ConEvent
    {
        public string Name { get; set; }
    }
    public class DeadEvent
    {
        public string Name { get; set; }
    }
    public class StartTimerEvent
    {
        public CustomTimer CustomTimer { get; set; }
    }

    public class CancelTimerEvent
    {
        public string Name { get; set; }
    }
    public class WhoPlayerEvent
    {
        public EQToolShared.APIModels.PlayerControllerModels.Player PlayerInfo { get; set; }
    }
    public class FailedFeignEvent
    {
        public string PersonWhoFailedFeign { get; set; }
    }
    public class GroupInviteEvent
    {
        public string Inviter { get; set; }
    }
    public class LevelEvent
    {
        public int NewLevel { get; set; }
    }
    public class DeathTouchEvent
    {
        public string NpcName { get; set; }
        public string DTReceiver { get; set; }
    }

    public class ResistSpellEvent
    {
        public Spell Spell { get; set; }
        public bool isYou { get; set; }
    }

    public class RandomRollEvent
    {
        public string PlayerName { get; set; }
        public int MaxRoll { get; set; }
        public int Roll { get; set; }
    }
    public class CompleteHealEvent
    {
        public string Recipient { get; set; }
        public string RecipientGuild { get; set; }
        public string Position { get; set; }
        public string Caster { get; set; }
    }

    public class SpellWornOffEvent
    {
        public string SpellName { get; set; }
    }

    public class SpellWornOffSelfEvent
    {
        public List<string> SpellNames { get; set; }
    }
    public class SpellWornOffOtherEvent
    {
        public string SpellName { get; set; }
    }

    public class SpellCastEvent
    {
        public string TargetName { get; set; }

        public bool IsYou { get; set; }

        public Spell Spell { get; set; }

        public bool MultipleMatchesFound { get; set; }

        public int? TotalSecondsOverride { get; set; }
    }

    public class YouZonedEvent
    {
        public string ZoneName { get; set; }
    }
}
