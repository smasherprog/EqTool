﻿using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Models
{
    public class CustomTimer
    {
        public const string CustomerTime = " Custom Timer";
        public const string ScoutTime = "Scout Charisa Timer";
        public const string Ring8 = "Ring 8 Roll Timer";
        
        public string Name { get; set; }
        public int Roll { get; set; } = -1;
        public int DurationSeconds { get; set; }
        public string SpellNameIcon { get; set; } = "Feign Death";
        public SpellBenefitDetriment SpellType { get; set; } = SpellBenefitDetriment.Beneficial;
        public string TargetName { get; set; } = CustomerTime;
        public Dictionary<PlayerClasses, int> Classes { get; set; } = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().Select(a => new { key = a, level = 1 }).ToDictionary(a => a.key, a => a.level);

        // todo - need to teach custom timer how to handle warnings and ending messages

        // fields ISO warning messages
        public int WarningSeconds { get; set; } = -1;
        public bool ProvideWarningText { get; set; } = false;
        public bool ProvideWarningTTS { get; set; } = false;
        public string WarningText { get; set; } = "";
        public string WarningTTS { get; set; } = "";

        // fields ISO ending message
        public bool ProvideEndText { get; set; } = false;
        public bool ProvideEndTTS { get; set; } = false;
        public string EndText { get; set; } = "";
        public string EndTTS { get; set; } = "";

        // restart any matching existing timer, or start new
        public bool RestartExisting { get; set; } = true;
    }
}
