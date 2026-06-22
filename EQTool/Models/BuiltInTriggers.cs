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
                CreateTellsYou()
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
            return Build("Charm Break", "Your charm spell has worn off.", false, "Charm Break", "Charm Break");
        }

        // "<name> tells you, '...'" -> tell alert.
        public static Trigger CreateTellsYou()
        {
            return Build("Tells You", "{name} tells you, ", true, "{name} sent a tell", "{name} sent a tell");
        }
        public static Trigger CreateEnraged()
        {
            return Build("Enraged", "{npc} has become ENRAGED.", true, "{npc} ENRAGED", "{npc} is enraged");
        }

        // "You feel as if you are about to fall." -> "Levitate Fading".
        public static Trigger CreateLevitateFading()
        {
            return Build("Levitate Fading", "You feel as if you are about to fall.", false, "Levitate Fading", "Levitate Fading");
        }

        // "You feel yourself starting to appear." -> "Invisability Fading.".
        public static Trigger CreateInvisFading()
        {
            return Build("Invis Fading", "You feel yourself starting to appear.", false, "Invisability Fading.", "Invisability Fading.");
        }

        // "<name> has fallen to the ground." -> failed feign death alert.
        public static Trigger CreateFailedFeign()
        {
            return Build("Failed Feign", "{name} has fallen to the ground.", true, "{name} Feign Failed Death!", "{name} Failed Feign Death");
        }

        // "<name> invites you to join a group." -> group invite alert.
        public static Trigger CreateGroupInvite()
        {
            return Build("Group Invite", "{name} invites you to join a group.", true, "{name} Invites you to a group", "{name} Invites you to a group");
        }

        // "<npc> begins to cast the gate spell." -> npc gating alert.
        public static Trigger CreateNpcGating()
        {
            return Build("NPC Gating", "{npc} begins to cast the gate spell.", true, "{npc} begins to Gate", "{npc} begins to Gate");
        }

        private static Trigger Build(string name, string searchText, bool useRegex, string displayText, string ttsText)
        {
            return new Trigger
            {
                IsBuiltIn = true,
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
