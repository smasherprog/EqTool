using EQToolShared.Enums;
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
        public int LineCounter { get; set; }
    }

    public class ExpGainedEvent : BaseLogParseEvent
    {
        // ctor
        public ExpGainedEvent() { }
        public ExpGainedEvent(DateTime timestamp, string line, int lineCounter)
        {
            TimeStamp = timestamp;
            Line = line;
            LineCounter = lineCounter;
        }
    }

    public enum FactionStatus
    {
        GotBetter,
        GotWorse,
        CouldNotGetBetter,
        CouldNotGetWorse
    }

    public class FactionEvent : BaseLogParseEvent
    {
        public string Faction { get; set; }
        public FactionStatus FactionStatus { get; set; }
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
    // Event class to carry all info from DamageParser to interested listeners
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

    public class SlainEvent : BaseLogParseEvent
    {
        public string Victim { get; set; }

        public string Killer { get; set; }
    }

    public class ConfirmedDeathEvent : BaseLogParseEvent
    {
        public string Victim { get; set; }

        public string Killer { get; set; }
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

    //
    // event class to carry all relevant communications events to interested listeners
    //
    public class CommsEvent : BaseLogParseEvent
    {
        public enum Channel
        {
            NONE = 0b_0000_0000,
            TELL = 0b_0000_0001,
            SAY = 0b_0000_0010,
            GROUP = 0b_0000_0100,
            GUILD = 0b_0000_1000,
            AUCTION = 0b_0001_0000,
            OOC = 0b_0010_0000,
            SHOUT = 0b_0100_0000,
            ANY = TELL | SAY | GROUP | GUILD | AUCTION | OOC | SHOUT
        }

        public CommsEvent.Channel TheChannel { get; set; }
        public string Content { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
    }

    public class YouBeginCastingEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
    }

    public class YouFinishCastingEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
        public string TargetName { get; set; }
    }

    public class SpellCastOnYouEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
    }

    public class SpellCastOnOtherEvent : BaseLogParseEvent
    {
        public List<Spell> Spells { get; set; }
        public string TargetName { get; set; }
    }

    public class ResistSpellEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
        public bool isYou { get; set; }
    }

    public class PayerChangedEvent : BaseLogParseEvent
    {
    }

    public class ClassDetectedEvent : BaseLogParseEvent
    {
        public PlayerClasses PlayerClass { get; set; }
    }
    public class PlayerLevelDetectionEvent : BaseLogParseEvent
    {
        public int PlayerLevel { get; set; }
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
        public string Tag { get; set; }
        public string Position { get; set; }
        public string Caster { get; set; }
    }
    public class YouHaveFinishedMemorizingEvent : BaseLogParseEvent
    {
        public string SpellName { get; set; }
    }

    public class YourSpellInterupptedEvent : BaseLogParseEvent
    {
    }
    public class DragonRoarEvent : BaseLogParseEvent
    {
        public Spell Spell { get; set; }
    }
    public class BaseRemoteEvent
    {
        public Point3D? Location { get; set; }
    }

    public class DragonRoarRemoteEvent : BaseRemoteEvent
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

    public class YouZonedEvent : BaseLogParseEvent
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
    }
    public class MendWoundsEvent : BaseLogParseEvent
    {
    }

    public class LineEvent : BaseLogParseEvent
    {
    }

    public class OverlayEvent
    {
        public string Text { get; set; }
        public System.Windows.Media.Brush ForeGround { get; set; }
        public bool Reset { get; set; }
    }
}
