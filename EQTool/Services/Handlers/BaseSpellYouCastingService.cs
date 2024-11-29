using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class BaseSpellYouCastingHandler
    {
        private readonly List<string> SpellsThatNeedTimers = new List<string>()
        {
            "Dictate",
            "Harvest",
            "Divine Aura",
            "Divine Barrier",
            "Harmshield",
            "Quivering Veil of Xarn",
            "Boon of the Garou"
        };

        private readonly List<string> SpellsThatNeedCounts = new List<string>()
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
            "Rage of Vallon",
            "Waves of the Deep Sea",
            "Anarchy"
        };
         
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
         
        public BaseSpellYouCastingHandler(  SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer)
        { 
            this.spellWindowViewModel = spellWindowViewModel; 
            this.activePlayer = activePlayer;
        }

        public void Handle(Spell spell, string targetName, int delayOffset, DateTime timestamp)
        {
            var spellname = spell.name;
            if (SpellsThatNeedTimers.Contains(spell.name))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = targetName,
                    Name = spellname,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)((spell.recastTime + delayOffset) / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)((spell.recastTime + delayOffset) / 1000.0)),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
            }
            else if (spell.name.EndsWith("Discipline"))
            {
                var basetime = (int)((spell.recastTime + delayOffset) / 1000.0);
                var playerlevel = activePlayer.Player.Level;
                if (spell.name == "Evasive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 51;
                    float secondsrange = (15 - 7) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 52;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Defensive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 54;
                    float secondsrange = (15 - 10) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 55;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Precision Discipline")
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
                    Type = spell.type,
                    SpellType = spell.SpellType,
                    GroupName = targetName,
                    Name = spellname,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    Classes = spell.Classes,
                    TotalDuration = TimeSpan.FromSeconds(basetime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(basetime),
                };
                spellWindowViewModel.TryAdd(vm);
            }

            var grpname = targetName;
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
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    Count = 1,
                    ProgressBarColor = Brushes.OrangeRed
                };
                spellWindowViewModel.TryAdd(vm);
            }
            else
            {
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                spellduration = spellduration.Add(TimeSpan.FromMilliseconds(delayOffset));
                if (spellduration.TotalSeconds >0)
                { 
                    var vm = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        Type = spell.type,
                        SpellType = spell.SpellType,
                        GroupName = grpname,
                        Name = spellname,
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        Classes = spell.Classes,
                        TotalDuration = spellduration,
                        TotalRemainingDuration = spellduration
                    };
                    spellWindowViewModel.TryAdd(vm);
                }
            }
        }
    }
}
