using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellHandlerService
    {
        // spells with long recast times, that need a cooldown timer
        public static readonly List<string> SpellsThatNeedTimers = new List<string>()
        {
            "Divine Aura",
            "Divine Barrier",
            "Harmshield",
            "Quivering Veil of Xarn",
        };

        public static readonly List<string> SpellsThatNeedTimersUnderYou = new List<string>()
        {
            "Dictate",
            "Harvest",
            "Boon of the Garou",
            "Theft of Thought"
        };

        // spells that we wish to count how many times they have been cast
        public static readonly List<string> SpellsThatNeedCounts = new List<string>()
        {
            "Mana Sieve",
            "LowerElement",
            "Concussion",
            "Flame Lick",
            "Jolt",
            "Cinder Jolt",
            "Rage of Vallon",
            "Waves of the Deep Sea",
            "Anarchy",
            "Breath of the Sea",
            "Frostbite",
            "Judgment of Ice",
            "Storm Strike",
            "Shrieking Howl",
            "Static Strike",
            "Rage of Zek",
            "Blinding Luminance",
            "Flash of Light"
        };

        // all the charm spells
        public static List<string> AllCharmSpells = new List<string>()
        {
            "Dictate",
            "Charm",
            "Beguile",
            "Cajoling Whispers",
            "Allure",
            "Boltran's Agacerie",
            "Befriend Animal",
            "Charm Animals",
            "Beguile Plants",
            "Beguile Animals",
            "Allure of the Wild",
            "Call of Karana",
            "Tunare's Request",
            "Dominate Undead",
            "Beguile Undead",
            "Cajole Undead",
            "Thrall of Bones",
            "Enslave Death"
        };

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly PlayerTrackerService playerTrackerService;

        public SpellHandlerService(PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer)
        {
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.activePlayer = activePlayer;
        }

        public void Handle(Spell spell, string targetName, int delayOffset, DateTime timestamp)
        {
            var targetclass = playerTrackerService.GetPlayer(targetName)?.PlayerClass;
            var spellname = spell.name;
            if (SpellsThatNeedTimersUnderYou.Any(a => string.Equals(spell.name, a, StringComparison.OrdinalIgnoreCase)))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = EQSpells.SpaceYou,
                    TargetClass = targetclass,
                    Name = $"{spellname} Cooldown",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)((spell.recastTime + delayOffset) / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)((spell.recastTime + delayOffset) / 1000.0)),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
            }
            else if (SpellsThatNeedTimers.Any(a => string.Equals(spell.name, a, StringComparison.OrdinalIgnoreCase)))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = targetName,
                    TargetClass = targetclass,
                    Name = $"{spellname} Cooldown",
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
                    float levelrange = 60 - 52;
                    float secondsrange = (15 - 7) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 52;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Defensive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 55;
                    float secondsrange = (15 - 10) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 55;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Precision Discipline")
                {
                    float baseseconds = 30 * 60;
                    float levelrange = 60 - 57;
                    float secondsrange = (30 - 27) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 57;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Stonestance Discipline")
                {
                    float baseseconds = 12 * 60;
                    float levelrange = 60 - 51;
                    float secondsrange = (12 - 4) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 51;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Voiddance Discipline")
                {
                    float baseseconds = 60 * 60;
                    float levelrange = 60 - 54;
                    float secondsrange = (60 - 54) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 54;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Innerflame Discipline")
                {
                    float baseseconds = 60 * 60;
                    float levelrange = 60 - 56;
                    float secondsrange = (30 - 26) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 56;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = targetName,
                    TargetClass = targetclass,
                    Name = spellname + " Cooldown",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(basetime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(basetime),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
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
                    TargetClass = targetclass,
                    Name = $"{spellname}",
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
                if (spellduration.TotalSeconds > 0)
                {
                    // set all beneficial spell types to overwrite/refresh, and all detrimental types to create multiple timers
                    var overWrite = true;
                    if (spell.benefit_detriment == EQToolShared.Enums.SpellBenefitDetriment.Detrimental)
                    {
                        overWrite = false;
                        spellduration = spellduration.Add(TimeSpan.FromMilliseconds(6000));
                    }

                    var vm = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        BenefitDetriment = spell.benefit_detriment,
                        SpellType = spell.SpellType,
                        GroupName = grpname,
                        TargetClass = targetclass,
                        Name = spellname,
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        Classes = spell.Classes,
                        TotalDuration = spellduration,
                        TotalRemainingDuration = spellduration
                    };


                    spellWindowViewModel.TryAdd(vm, overWrite);
                }
            }
        }
    }
}
