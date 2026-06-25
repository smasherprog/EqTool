using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Models
{
    public static class BuiltInTriggers
    {
        public const string CategoryName = "Built In";

        // Ensures every built-in trigger exists in the user's persisted trigger list and is
        // enabled. This runs at program startup so newly shipped built-in triggers light up
        // automatically. A built-in is considered already present when the user already has a
        // trigger carrying the same BuiltInId (which survives copy/cut), so renaming or editing
        // a copied built-in still counts as present and it is left untouched - preserving any
        // edits or manual disables the user made. Only genuinely missing built-ins are added
        // (enabled). Returns true when something was added so the caller can persist.
        public static bool EnsurePresentAndEnabled(EQToolSettings settings)
        {
            if (settings == null)
            {
                return false;
            }
            if (settings.Triggers == null)
            {
                settings.Triggers = new List<Trigger>();
            }

            var changed = false;
            foreach (var builtIn in All())
            {
                // Recognize an existing copy by its stable BuiltInId.
                var existing = settings.Triggers.FirstOrDefault(t =>
                    !string.IsNullOrEmpty(t.BuiltInId) &&
                    string.Equals(t.BuiltInId, builtIn.BuiltInId, StringComparison.OrdinalIgnoreCase));

                // Fall back to the legacy "Built In" category + name match for copies added
                // before BuiltInId existed, and backfill the id so later runs and duplicate
                // detection can rely on it.
                if (existing == null)
                {
                    existing = settings.Triggers.FirstOrDefault(t =>
                        string.IsNullOrEmpty(t.BuiltInId) &&
                        string.Equals(t.Category, CategoryName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(t.TriggerName, builtIn.TriggerName, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.BuiltInId = builtIn.BuiltInId;
                        changed = true;
                    }
                }

                if (existing != null)
                {
                    continue;
                }

                // Copy it in as a normal, editable, enabled trigger living at the top level of
                // the Triggers tree (mirrors what happens when a user copies one out of the
                // read-only Built In library).
                builtIn.IsBuiltIn = false;
                builtIn.TriggerEnabled = true;
                builtIn.FolderId = null;
                settings.Triggers.Add(builtIn);
                changed = true;
            }
            return changed;
        }

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
            // search text to use after {c} macro is added to the regex engine
            // return Build("builtin:tells-you", "Tells You", "{name}(tells you, '| -> {c}: )(?!I'll give you|Attacking |Welcome to my bank|Come back soon)", true, "{name} sent a tell", "{name} sent a tell");
            return Build("builtin:tells-you", "Tells You", "{name}(tells you, '| ->)(?!I'll give you|Attacking |Welcome to my bank|Come back soon)", true, "{name} sent a tell", "{name} sent a tell");
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
