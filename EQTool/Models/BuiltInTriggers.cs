using System;
using System.Collections.Generic;

namespace EQTool.Models
{
    public static class BuiltInTriggers
    {
        public const string CategoryName = "Built In";

        public static List<Trigger> All()
        {
            return new List<Trigger>
            {
                CreateEnraged(),
                CreateLevitateFading(),
                CreateInvisFading(),
                CreateFailedFeign(),
                CreateGroupInvite(),
                CreateNpcGating(),
                CreateCharmBreak(),
                CreateDeathTouch(),
                CreateTellsYou(),
                CreateResist(),
                CreateAvatarOfWarLockout(),
                CreateVPHoskarResto(),
                CreateSpellWornOff(),
                CreateEnteredZone(),

                // Spell/combat feedback triggers ported from the old hardcoded trigger list.
                // These only show overlay text (no audio) but keep their original audio text so
                // the user can switch audio on if they want it.
                BuildLegacy("builtin:spell-interrupted", "Spell Interrupted", "^Your spell is interrupted.", "Spell Interrupted", "Interrupted"),
                BuildLegacy("builtin:spell-fizzle", "Spell Fizzle", "^Your spell fizzles!", "Spell Fizzles", "Fizzle"),
                BuildLegacy("builtin:backstabber", "Backstabber", "^{backstabber} backstabs {target} for {damage} points of damage.", "{backstabber} backstabs {target} for {damage}", "Backstabber"),
                BuildLegacy("builtin:corpse-need-consent", "Corpse Need Consent", "^You do not have consent to summon that corpse", "Need Consent", "Need Consent"),
                BuildLegacy("builtin:corpse-out-of-range", "Corpse Out of Range", "^The corpse is too far away to summon", "Corpse OOR", "Corpse out of range"),
                BuildLegacy("builtin:select-a-target", "Select a Target", "^(You must first select a target for this spell)|(You must first click on the being you wish to attack)", "Select a target", "Select a target"),
                BuildLegacy("builtin:insufficient-mana", "Insufficient Mana", "^Insufficient Mana to cast this spell!", "OOM", "out of mana"),
                BuildLegacy("builtin:target-out-of-range", "Target Out of Range", "^Your target is out of range", "Target out of range", "Out of range"),
                BuildLegacy("builtin:spell-did-not-take-hold", "Spell Did Not Take Hold", "^Your spell did not take hold", "Spell did not take hold", "Spell did not take hold"),
                BuildLegacy("builtin:must-be-standing", "Must be standing to cast", "^(You must be standing)|(You are too distracted to cast a spell now)", "Stand up!", "stand up"),
                BuildLegacy("builtin:dispelled", "Dispelled", "^You feel a bit dispelled", "You have been dispelled", "dispelled"),
                BuildLegacy("builtin:regen-faded", "Regen Faded", "^You have stopped regenerating", "===== Regen faded =====", "re-gen faded"),
                BuildLegacy("builtin:cant-see-target", "Can't See Target", "^You can't see your target", "Can't see target", "Can't see target"),
                BuildLegacy("builtin:sense-heading", "Sense Heading", "^You think you are heading {direction}", "Direction = {direction}", "{direction}"),
                BuildLegacy("builtin:sense-heading-failed", "Sense Heading Failed", "^You have no idea what direction you are facing", "No idea", "no idea")
            };
        }

        // "Fright says, 'Targetname'" / "Dread says, 'Targetname'" -> 45 second death touch timer.
        // Fright and Dread announce their target by name (a single word) just before death touching it.
        // Ported from the old DeathTouchHandler: shows a 45 second countdown named after the target.
        public static Trigger CreateDeathTouch()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:death-touch",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Death Touch (Fright/Dread)",
                // real regex (not the simplified {name} form) so the target stays a single word
                SearchText = @"^(?<npc>Fright|Dread) says,? '(?<target>[^' ]+)'",
                UseRegex = true,
                Category = CategoryName,
                Zone = "fear",
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "Death Touch: {target}",
                    DisplayTextColor = "White",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "Death touch on {target}"
                },
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "--DT-- '{target}'",
                    Seconds = 45,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "DarkRed"
                }
            };
        }

        public static Trigger CreateResist()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:resist",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Resist",
                SearchText = @"^(?:You resist the|Your target resisted the) (?<spell>.+) spell[!.]",
                UseRegex = true,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "Resisted: {spell}",
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "Resisted"
                }
            };
        }

        public const string AvatarOfWarBuiltInId = "builtin:aow-lockout";
        public static Trigger CreateAvatarOfWarLockout()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = AvatarOfWarBuiltInId,
                BuiltInFolder = "Encounters/Kael",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Avatar of War Lockout",
                SearchText = "The Avatar of War shouts 'Who dares defile my temple?! Come forth and face me!'",
                UseRegex = false,
                Category = CategoryName,
                Zone = "kael",
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "The Avatar of War Lockout",
                    Minutes = 20,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Orchid",
                    IconName = "Spirit of Wolf"
                }
            };
        }

        // Hoskar (VP) casts Diseased Cloud, after which you have ~8 seconds to land Word of
        // Restoration. Fires on the Diseased Cloud cast (on you or others) or a resist of it,
        // but only while in Veeshan's Peak. Shows an 8s "Word Of Resto" countdown, then alerts
        // "Resto Now" when it ends. Ported from the old DiseasedCloudHandler. Lives under the
        // Built In > Encounters > VP folder, and is auto-enabled for users on startup
        // (see EQToolSettingsLoad.EnableVPHoskarRestoTriggerIfMissing).
        public const string VPHoskarRestoBuiltInId = "builtin:vp-hoskar-resto";
        public static Trigger CreateVPHoskarResto()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = VPHoskarRestoBuiltInId,
                BuiltInFolder = "Encounters/VP",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Hoskar Resto",
                // Diseased Cloud cast on you ("Your body begins to rot.") or others
                // ("<name>'s body begins to rot."), or a resist of it.
                SearchText = @"(body begins to rot\.|resist(ed)? the Diseased Cloud spell)",
                UseRegex = true,
                Category = CategoryName,
                Zone = "veeshan",
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "Word Of Resto",
                    Seconds = 8,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "DarkGreen",
                    IconName = "Diseased Cloud",
                    ShowInOverlay = true
                },
                TimerEnded = new TriggerTimerEnded
                {
                    Enabled = true,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "Resto Now",
                        DisplayTextColor = "Yellow",
                        AudioType = TriggerAudioType.TextToSpeech,
                        TtsText = "Resto Now"
                    }
                }
            };
        }

        // "Your <spell> spell has worn off." -> "<spell> faded" alert. Ported from the alert
        // portion of SpellWornOffOtherHandler (which still handles removing the faded spell from
        // the timer window). Auto-enabled for users on startup
        // (see EQToolSettingsLoad.EnableSpellWornOffTriggerIfMissing).
        public const string SpellWornOffBuiltInId = "builtin:spell-worn-off";
        public static Trigger CreateSpellWornOff()
        {
            return Build(SpellWornOffBuiltInId, "Spell Worn Off", @"^Your (?<spell>[\w ]+) spell has worn off\.", true, "{spell} faded", "{spell} faded");
        }

        // "You have entered <zone>." -> "You zoned into <zone>" alert. Ported from the alert
        // portion of YouZonedHandler (which still tracks the player's current zone). The negative
        // lookahead skips the non-zone "You have entered an Arena (PvP) area." / "...an area where..."
        // messages so only real zone changes alert.
        public const string EnteredZoneBuiltInId = "builtin:entered-zone";
        public static Trigger CreateEnteredZone()
        {
            return Build(EnteredZoneBuiltInId, "Entered Zone", @"^You have entered (?!an Arena|an area)(?<zone>.+)\.", true, "You zoned into {zone}", "You zoned into {zone}");
        }

        // "Your charm spell has worn off." -> charm break alert.
        public static Trigger CreateCharmBreak()
        {
            return Build("builtin:charm-break", "Charm Break", "Your charm spell has worn off.", false, "Charm Break", "Charm Break");
        }

        // "<name> tells you, '...'" -> tell alert.
        public static Trigger CreateTellsYou()
        {
            return Build("builtin:tells-you", "Tells You", "{name}(tells you, '| -> {c}: )(?!I'll give you|Attacking |Welcome to my bank|Come back soon)", true, "{name} sent a tell", "{name} sent a tell");
        }
        public static Trigger CreateEnraged()
        {
            return Build("builtin:enraged", "Enraged", "{npc} has become ENRAGED.", true, "{npc} ENRAGED", "{npc} is enraged");
        }

        // "You feel as if you are about to fall." -> "Levitate Fading".
        public static Trigger CreateLevitateFading()
        {
            return Build("builtin:levitate-fading", "Levitate Fading", "You feel as if you are about to fall.", false, "Levitate Fading", "Levitate Fading");
        }

        // "You feel yourself starting to appear." -> "Invisability Fading.".
        public static Trigger CreateInvisFading()
        {
            return Build("builtin:invis-fading", "Invis Fading", "You feel yourself starting to appear.", false, "Invisability Fading.", "Invisability Fading.");
        }

        // "<name> has fallen to the ground." -> failed feign death alert.
        public static Trigger CreateFailedFeign()
        {
            return Build("builtin:failed-feign", "Failed Feign", "{name} has fallen to the ground.", true, "{name} Feign Failed Death!", "{name} Failed Feign Death");
        }

        // "<name> invites you to join a group." -> group invite alert.
        public static Trigger CreateGroupInvite()
        {
            return Build("builtin:group-invite", "Group Invite", "{name} invites you to join a group.", true, "{name} Invites you to a group", "{name} Invites you to a group");
        }

        // "<npc> begins to cast the gate spell." -> npc gating alert.
        public static Trigger CreateNpcGating()
        {
            return Build("builtin:npc-gating", "NPC Gating", "{npc} begins to cast the gate spell.", true, "{npc} begins to Gate", "{npc} begins to Gate");
        }

        // A display-only built-in: shows overlay text and never speaks. The audio text is still
        // stored (with AudioType None) so the user can flip on TTS later without retyping it.
        private static Trigger BuildLegacy(string builtInId, string name, string searchText, string displayText, string audioText)
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = builtInId,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = name,
                SearchText = searchText,
                UseRegex = true,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = displayText,
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.None,
                    TtsText = audioText
                }
            };
        }

        private static Trigger Build(string builtInId, string name, string searchText, bool useRegex, string displayText, string ttsText)
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = builtInId,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = name,
                SearchText = searchText,
                UseRegex = useRegex,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = displayText,
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = ttsText
                }
            };
        }
    }
}
