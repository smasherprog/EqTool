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
                CreateExpGained(),

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

        // create trigger exp grinding
        //  - triggers from "You gain experience" or "You gain party experience"
        //  - uses {counter} as part of output, to help user keep track of spawns
        //  - allow user to send a tell to ".exp" to immediately clear all timers of this type
        public static Trigger CreateExpGained()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                BuiltInId = "builtin:exp-gained-0640",
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Exp Timer // ^You gain (party )?experience",
                // real regex (not the simplified {name} form) so the target stays a single word
                SearchText = @"^You gain (party )?experience",
                UseRegex = true,
                Category = CategoryName,
                Comments =  "Note: Sending a tell to \".exp\", i.e. \"/t .exp\" will immediately terminate all timers of this type!\n" +
                            "BB fishers = 6:40 and 22:00, " +
                            "Chardok = 18:00, " +
                            "COM = 22:00, " +
                            "Crystal Caverns = 14:45, " +
                            "Droga = 20:30, " +
                            "Grobb = 24:00, " +
                            "Hole = 21:30, " +
                            "HS = 20:30, " +
                            "Kael = 28:00, " +
                            "Kedge = 22:00, " +
                            "Lower Guk = 28:00, " +
                            "MM = 23 min, " +
                            "North Felwithe guards = 24:00, " +
                            "Oasis specs = 16:30, " +
                            "OOT specs/sisters = 6:00, " +
                            "Paw = 22:00, " +
                            "Perma = 22:00, " +
                            "Seb Lair = 27:00, " +
                            "Skyfire = 13:00, " +
                            "Skyshrine = 30:00, " +
                            "TD = 12:00, " +
                            "TT = 6:40, " +
                            "Wars Woods brutes = 6:40, " +
                            "WK guards = 6:00, " +
                            "WL = 14:30, ",

                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = false,
                    AudioType = TriggerAudioType.None,
                    TtsText = ""
                },
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "-- Exp Timer [{counter}] (.exp)",
                    Minutes = 6,
                    Seconds = 40,
                    RestartBehavior = TimerRestartBehavior.StartNewTimer,
                    EndEarlyTexts = new System.Collections.ObjectModel.ObservableCollection<EndEarlyEntry> 
                    {
                        new EndEarlyEntry 
                        {
                            SearchText = @"^\.exp",
                            UseRegex = true,
                        },
                    }
                },
                TimerEnding = new TriggerTimerEnding
                {
                    Enabled = true,
                    Seconds = 30,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "30 second warning",
                        AudioType = TriggerAudioType.TextToSpeech, 
                        TtsText = "30 second warning"
                    }
                },
                TimerEnded = new TriggerTimerEnded
                {
                    Enabled = true,
                    Output = new TriggerOutput
                    {
                        DisplayTextEnabled = true,
                        DisplayText = "Pop [{counter}]",
                        AudioType = TriggerAudioType.TextToSpeech,
                        TtsText = "Pop"
                    }
                },
                Counter = new TriggerCounter
                {
                    ResetEnabled = true,
                    Minutes = 30,
                }
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
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "Death Touch: {target}",
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "Death touch on {target}"
                },
                Timer = new TriggerTimer
                {
                    TimerType = TimerType.CountDown,
                    TimerName = "--DT-- '{target}'",
                    Seconds = 45,
                    RestartBehavior = TimerRestartBehavior.RestartTimer,
                    BarColor = "Red"
                }
            };
        }

        // "Your charm spell has worn off." -> charm break alert.
        public static Trigger CreateCharmBreak()
        {
            return Build("builtin:charm-break", "Charm Break", "Your charm spell has worn off.", false, "Charm Break", "Charm Break");
        }

        // "<name> tells you, '...'" -> tell alert.
        public static Trigger CreateTellsYou()
        {
            return Build("builtin:tells-you", "Tells You", "{name}( tells you, '| -> {c}: )(?!I'll give you|Attacking |Welcome to my bank|Come back soon)", true, "{name} sent a tell", "{name} sent a tell");
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
            return Build("builtin:failed-feign", "Failed Feign", "{c} has fallen to the ground.", true, "{c} Feign Failed Death!", "{c} Failed Feign Death");
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
