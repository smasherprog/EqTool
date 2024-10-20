using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace EQTool.Models
{
    //
    // base class
    // all Events know at least about the timestamp and the line from the logfile
    // child Event class can add any specific fields needed to carry additional info
    //
    public class BaseLogParseEvent
    {
        public DateTime TimeStamp { get; set; }
        public string Line { get; set; }
    }

    public class PlayerLocationEvent : BaseLogParseEvent
    {
        public Point3D Location { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
    }
    public class WhoEvent : BaseLogParseEvent { }
    public class CampEvent : BaseLogParseEvent { }
    public class EnteredWorldEvent : BaseLogParseEvent { }
    public class QuakeEvent : BaseLogParseEvent { }
    public class CharmBreakEvent : BaseLogParseEvent { }
    public class InvisEvent : BaseLogParseEvent
    {
        public Services.Parsing.InvisParser.InvisStatus InvisStatus { get; set; }
    }
    public class LevitateEvent : BaseLogParseEvent
    {
        public Services.Parsing.LevParser.LevStatus LevitateStatus { get; set; }
    }
    public class FTEEvent : BaseLogParseEvent
    {
        public string NPCName { get; set; }
        public string FTEPerson { get; set; }
    }
    public class EnrageEvent : BaseLogParseEvent
    {
        public string NpcName { get; set; }
    }

    //
    // Event class to carry all info from DamageLogParser to interested listeners
    //
    public class DamageEvent : BaseLogParseEvent
    {
        public string TargetName { get; set; }
        public string AttackerName { get; set; }
        public int DamageDone { get; set; }
        public string DamageType { get; set; }
    }

    public class ConEvent : BaseLogParseEvent
    {
        public string Name { get; set; }
    }
    public class DeadEvent : BaseLogParseEvent
    {
        public string Name { get; set; }
    }
    public class StartTimerEvent : BaseLogParseEvent
    {
        public CustomTimer CustomTimer { get; set; }
    }

    public class CancelTimerEvent : BaseLogParseEvent
    {
        public string Name { get; set; }
    }
    public class WhoPlayerEvent : BaseLogParseEvent
    {
        public EQToolShared.APIModels.PlayerControllerModels.Player PlayerInfo { get; set; }
    }
    public class FailedFeignEvent : BaseLogParseEvent
    {
        public string PersonWhoFailedFeign { get; set; }
    }
    public class GroupInviteEvent : BaseLogParseEvent
    {
        public string Inviter { get; set; }
    }
    public class LevelEvent : BaseLogParseEvent
    {
        public int NewLevel { get; set; }
    }
    public class DeathEvent : BaseLogParseEvent { }

    public class PlayerCommsEvent : BaseLogParseEvent
    {
        public enum Channel
        {
            NONE        = 0b_0000_0000,
            TELL        = 0b_0000_0001,
            SAY         = 0b_0000_0010,
            GROUP       = 0b_0000_0100,
            GUILD       = 0b_0000_1000,
            AUCTION     = 0b_0001_0000,
            OOC         = 0b_0010_0000,
            SHOUT       = 0b_0100_0000,
            ANY         = TELL|SAY|GROUP|GUILD|AUCTION|OOC|SHOUT
        }

        public PlayerCommsEvent.Channel theChannel { get; set; }
    }


    public class DeathTouchEvent : BaseLogParseEvent
    {
        public string NpcName { get; set; }
        public string DTReceiver { get; set; }
    }

    public class ResistSpellEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
        public bool isYou { get; set; }
    }

    public class PayerChangedEvent : BaseLogParseEvent
    {
    }

    public class RandomRollEvent : BaseLogParseEvent
    {
        public string PlayerName { get; set; }
        public int MaxRoll { get; set; }
        public int Roll { get; set; }
    }
    public class CompleteHealEvent : BaseLogParseEvent
    {
        public string Recipient { get; set; }
        public string RecipientGuild { get; set; }
        public string Position { get; set; }
        public string Caster { get; set; }
    }

    public class SpellWornOffEvent : BaseLogParseEvent
    {
        public string SpellName { get; set; }
    }

    public class SpellWornOffSelfEvent : BaseLogParseEvent
    {
        public List<string> SpellNames { get; set; }
    }
    public class SpellWornOffOtherEvent : BaseLogParseEvent
    {
        public string SpellName { get; set; }
    }

    public class SpellCastEvent : BaseLogParseEvent
    {
        public string TargetName { get; set; }

        public bool CastByYou { get; set; }

        public Spell Spell { get; set; }

        public bool MultipleMatchesFound { get; set; }

        public int? TotalSecondsOverride { get; set; }
    }

    public class YouBeginCastingEvent : BaseLogParseEvent
    {
    }

    public class YouZonedEvent : BaseLogParseEvent
    {
        public string ZoneName { get; set; }
    }
}
