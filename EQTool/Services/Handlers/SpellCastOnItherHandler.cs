using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellCastOnItherHandler : BaseHandler
    {   

        private readonly List<string> SpellsThatNeedCounts = new List<string>()
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
        };

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public SpellCastOnItherHandler(
            EQSpells spells,
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent; 
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        { 
            var spellname = e.Spell.name;
            if (e.Spell.name.EndsWith("Discipline"))
            {
                var basetime = (int)(e.Spell.recastTime / 1000.0);
                var playerlevel = activePlayer.Player.Level;
                if (e.Spell.name == "Evasive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 51;
                    float secondsrange = (15 - 7) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 52;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (e.Spell.name == "Defensive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 54;
                    float secondsrange = (15 - 10) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 55;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (e.Spell.name == "Precision Discipline")
                {
                    float baseseconds = 30 * 60;
                    float levelrange = 60 - 56;
                    float secondsrange = (30 - 27) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 57;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }

                var vm = new SpellViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    PercentLeft = 100,
                    Type = e.Spell.type,
                    SpellType = e.Spell.SpellType,
                    GroupName = e.TargetName,
                    Name = spellname,
                    Rect = e.Spell.Rect,
                    Icon = e.Spell.SpellIcon,
                    Classes = e.Spell.Classes,
                    TotalDuration = TimeSpan.FromSeconds(basetime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(basetime),
                };
                spellWindowViewModel.TryAdd(vm);
            }

            var grpname = e.TargetName;
            if (MasterNPCList.NPCs.Contains(grpname))
            {
                grpname = " " + grpname;
            }
            var needscount = SpellsThatNeedCounts.Contains(spellname);
            if (needscount)
            {
                var vm = new CounterViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    GroupName = grpname,
                    Name = spellname,
                    Rect = e.Spell.Rect,
                    Icon = e.Spell.SpellIcon,
                    Count = 1,
                    ProgressBarColor = Brushes.OrangeRed
                };
                spellWindowViewModel.TryAdd(vm);
            }
            else
            {
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(e.Spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
 
                var vm = new SpellViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    PercentLeft = 100,
                    Type = e.Spell.type,
                    SpellType = e.Spell.SpellType,
                    GroupName = grpname,
                    Name = spellname,
                    Rect = e.Spell.Rect,
                    Icon = e.Spell.SpellIcon,
                    Classes = e.Spell.Classes,
                    TotalDuration = spellduration,
                    TotalRemainingDuration = spellduration
                };
                spellWindowViewModel.TryAdd(vm);
            }
        }
    }
}
