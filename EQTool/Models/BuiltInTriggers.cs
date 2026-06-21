using System;
using System.Collections.Generic;

namespace EQTool.Models
{
    // The read-only "Built In" trigger library. These are constructed in code every
    // session (never persisted) and can only be copied out into an editable category.
    public static class BuiltInTriggers
    {
        public const string CategoryName = "Built In";

        public static List<Trigger> All()
        {
            return new List<Trigger>
            {
                CreateEnraged()
            };
        }

        // Mirrors the existing Enrage alert: a line ending "... has become ENRAGED."
        // speaks "<npc> is enraged" and overlays "<npc> ENRAGED".
        public static Trigger CreateEnraged()
        {
            return new Trigger
            {
                IsBuiltIn = true,
                TriggerEnabled = false,
                TriggerId = Guid.NewGuid(),
                TriggerName = "Enraged",
                SearchText = "{npc} has become ENRAGED.",
                UseRegex = true,
                Category = CategoryName,
                Basic = new TriggerOutput
                {
                    DisplayTextEnabled = true,
                    DisplayText = "{npc} ENRAGED",
                    DisplayTextColor = "Red",
                    AudioType = TriggerAudioType.TextToSpeech,
                    TtsText = "{npc} is enraged"
                }
            };
        }
    }
}
