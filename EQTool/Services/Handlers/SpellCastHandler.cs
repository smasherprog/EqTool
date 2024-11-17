using EQTool.Models;
using EQTool.Services.Factories;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class SpellCastHandler : BaseHandler
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

        private readonly List<string> SpellsThatDragonsDo = new List<string>()
        {
            "Dragon Roar",
            "Silver Breath",
            "Ice breath",
            "Mind Cloud",
            "Rotting Flesh",
            "Putrefy Flesh",
            "Stun Breath",
            "Immolating Breath",
            "Rain of Molten Lava",
            "Frost Breath",
            "Lava Breath",
            "Cloud of Fear",
            "Diseased Cloud",
            "Tsunami",
            "Ancient Breath"
        };

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public SpellCastHandler(
            EQSpells spells,
            SpellWindowViewModel spellWindowViewModel,
            SpellWindowViewModelFactory spellWindowViewModelFactory,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SpellCastEvent += LogEvents_SpellCastEvent;
        }

        private void LogEvents_SpellCastEvent(object sender, SpellCastEvent match)
        {
            var spellname = match.Spell.name;
            if (string.Equals(match.Spell.name, "Harvest", StringComparison.OrdinalIgnoreCase) && match.TargetName == EQSpells.SpaceYou)
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = spellname,
                    Rect = match.Spell.Rect,
                    Icon = match.Spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(600),
                    TotalRemainingDuration = TimeSpan.FromSeconds(600),
                    UpdatedDateTime = DateTime.Now
                });
            }
            else if (SpellsThatDragonsDo.Contains(match.Spell.name))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = spellname,
                    Rect = match.Spell.Rect,
                    Icon = match.Spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)(match.Spell.recastTime / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)(match.Spell.recastTime / 1000.0)),
                    UpdatedDateTime = DateTime.Now
                });
            }
            else if (match.Spell.name.EndsWith("Discipline"))
            {
                var basetime = (int)(match.Spell.recastTime / 1000.0);
                var playerlevel = activePlayer.Player.Level;
                if (match.Spell.name == "Evasive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 51;
                    float secondsrange = (15 - 7) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 52;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (match.Spell.name == "Defensive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 54;
                    float secondsrange = (15 - 10) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 55;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (match.Spell.name == "Precision Discipline")
                {
                    float baseseconds = 30 * 60;
                    float levelrange = 60 - 56;
                    float secondsrange = (30 - 27) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = playerlevel - 57;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                var discspell = spells.AllSpells.FirstOrDefault(a => a.name == "Strengthen");
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = "--Discipline-- " + spellname,
                    Rect = discspell.Rect,
                    Icon = discspell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(basetime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(basetime),
                    UpdatedDateTime = DateTime.Now
                });
            }

            var needscount = SpellsThatNeedCounts.Contains(spellname);
            if (needscount)
            {
                var vm = new CounterViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    GroupName = match.TargetName,
                    Name = spellname,
                    Rect = match.Spell.Rect,
                    Icon = match.Spell.SpellIcon,
                    Count = 1
                };
                spellWindowViewModel.TryAdd(vm);
            }
            else
            {
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(match.Spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                if (spellname == "Mend")
                {
                    spellduration = TimeSpan.FromMinutes(6);
                }

                var vm = new SpellViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    PercentLeft = 100,
                    SpellType = match.Spell.type,
                    GroupName = match.TargetName,
                    Name = spellname,
                    Rect = match.Spell.Rect,
                    Icon = match.Spell.SpellIcon,
                    Classes = match.Spell.Classes,
                    GuessedSpell = match.MultipleMatchesFound,
                    TotalDuration = spellduration,
                    TotalRemainingDuration = spellduration
                };
                spellWindowViewModel.TryAdd(vm);
            }
        }
    }
}
