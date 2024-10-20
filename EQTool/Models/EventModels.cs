﻿using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace EQTool.Models
{
    public class BaseLogParseEvent
    {
        public DateTime TimeStamp { get; set; }
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

    public class FightHitEvent : BaseLogParseEvent
    {
        public DPSParseMatch HitInformation { get; set; }
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

    public class YouZonedEvent : BaseLogParseEvent
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
    }
}
