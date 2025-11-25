using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EQTool.Models;
using EQTool.ViewModels.SpellWindow;

namespace EQTool.Services
{
    public class SpellGroupingEngine
    {
        private readonly PlayerInfo activePlayer;
        private readonly EQToolSettings settings;

        // Lookups
        private readonly List<SpellViewModel> allSpells = new List<SpellViewModel>();
        private readonly Dictionary<string, List<SpellViewModel>> activeSpellIds = new Dictionary<string, List<SpellViewModel>>();

        // Branch-and-bound fields
        private SpellGroupInfo[] spellGroups;
        private List<int>[] targetsCoveredByGroup;
        private int[] remainingSpellsPerTarget;
        private string[] allTargets;
        private char[] groupSelectionMask;
        private HashSet<string> visitedStates;
        private int bestCategoryCount;
        private int bestNameSpellCount;
        private HashSet<string> bestChosenGroups;

        public SpellGroupingEngine(PlayerInfo activePlayer, EQToolSettings settings)
        {
            this.activePlayer = activePlayer;
            this.settings = settings;
        }

        public void AddSpells(IEnumerable<SpellViewModel> newSpells)
        {
            foreach (var spell in newSpells)
            {
                allSpells.Add(spell);
                activeSpellIds.SafelyAdd(spell.Id, spell);
            }
        }

        public void RemoveSpells(IEnumerable<SpellViewModel> removedSpells)
        {
            foreach (var spell in removedSpells)
            {
                allSpells.Remove(spell);
                activeSpellIds.SafelyRemove(spell.Id, spell);
            }
        }
        
        public void Recategorize(IEnumerable<SpellViewModel> deltaSpells = null)
        {
            var spellsToGroup = GetSpellsForGrouping(deltaSpells ?? allSpells);
            if (!spellsToGroup.Any())
                return;

            RunFullCategorization(spellsToGroup);
        }
        
        public void HandleNonConciseGroupingForSpell(SpellViewModel spell)
        {
            var groupingType = GetGroupingType(spell);
            if (groupingType == SpellGroupingType.ByTarget)
                spell.IsCategorizeById = false;
            else if (groupingType == SpellGroupingType.BySpell)
                spell.IsCategorizeById = true;
            else if (groupingType == SpellGroupingType.BySpellExceptYou)
                spell.IsCategorizeById = !spell.CastOnYou();
            
            // Automatic grouping handled elsewhere due to it needed to evaluate the whole list at once.
        }
        
        private IEnumerable<SpellViewModel> GetSpellsForGrouping(IEnumerable<SpellViewModel> baselineSpells)
        {
            var playerMode = settings.PlayerSpellGroupingType == SpellGroupingType.Automatic;
            var npcMode = settings.NpcSpellGroupingType == SpellGroupingType.Automatic;

            if (!playerMode && !npcMode)
                return Enumerable.Empty<SpellViewModel>();

            return baselineSpells.Where(s => s.ColumnVisibility == Visibility.Visible && ((s.IsPlayerTarget && playerMode) || (!s.IsPlayerTarget && npcMode)));
        }

        private SpellGroupingType GetGroupingType(SpellViewModel spell)
            => spell.IsPlayerTarget
                ? settings.PlayerSpellGroupingType
                : settings.NpcSpellGroupingType;

        private bool RequiresFullRecategorization(IEnumerable<SpellViewModel> deltaSpells)
        {
            foreach (var spell in deltaSpells)
            {
                if (!activeSpellIds.TryGetValue(spell.Id, out var existing))
                    continue;

                // If name exists elsewhere or matches player class, it can affect Name categories
                if (existing.Count > 1 || spell.CastByYourClass(activePlayer))
                    return true;
            }
            return false;
        }

        private void RunFullCategorization(IEnumerable<SpellViewModel> visibleSpells)
        {
            PrecomputeSpellGroups(visibleSpells); // only consider visible spells in grouping

            bestCategoryCount = int.MaxValue;
            bestNameSpellCount = -1;
            bestChosenGroups = null;

            groupSelectionMask = Enumerable.Repeat('0', spellGroups.Length).ToArray();
            visitedStates = new HashSet<string>(StringComparer.Ordinal);

            var totalUnassignedTargets = remainingSpellsPerTarget.Count(c => c > 0);
            RecurseSpellGroups(0, 0, 0, totalUnassignedTargets);

            if (bestChosenGroups == null)
                bestChosenGroups = new HashSet<string>();

            // Apply final categorization flags
            foreach (var spell in visibleSpells)
                spell.IsCategorizeById = bestChosenGroups.Contains(spell.Id);
        }

        // -----------------------
        // Branch-and-bound algorithm
        // -----------------------
        private void PrecomputeSpellGroups(IEnumerable<SpellViewModel> visibleSpells)
        {
            var groupedByName = visibleSpells.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.ToList());

            spellGroups = groupedByName.Values
                .Select(list =>
                {
                    var group = new SpellGroupInfo
                    {
                        Name = list[0].Id,
                        Spells = list,
                        SpellCount = list.Count,
                        DistinctTargetCount = list.Select(s => s.Target).Distinct().Count(),
                        MatchesPlayerClass = list.Any(s => s.CastByYourClass(activePlayer))
                    };
                    group.Impact = group.SpellCount - group.DistinctTargetCount;
                    return group;
                })
                .OrderByDescending(g => g.MatchesPlayerClass)
                .ThenByDescending(g => g.Impact)
                .ThenByDescending(g => g.SpellCount)
                .ToArray();

            allTargets = visibleSpells.Select(s => s.Target).Distinct().ToArray();
            var targetIndexMap = allTargets.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i, StringComparer.Ordinal);

            targetsCoveredByGroup = new List<int>[spellGroups.Length];
            remainingSpellsPerTarget = new int[allTargets.Length];

            for (var i = 0; i < spellGroups.Length; i++)
            {
                var targetIndices = spellGroups[i].Spells.Select(s => targetIndexMap[s.Target]).Distinct().ToList();
                targetsCoveredByGroup[i] = targetIndices;

                foreach (var idx in targetIndices)
                    remainingSpellsPerTarget[idx]++;
            }
        }

        private void RecurseSpellGroups(int groupIndex, int chosenGroupCount, int chosenSpellCount, int unassignedTargetCount)
        {
            var stateKey = groupIndex + "|" + new string(groupSelectionMask);
            if (!visitedStates.Add(stateKey))
                return;

            int lowerBoundCategoryCount = chosenGroupCount + unassignedTargetCount;
            if (lowerBoundCategoryCount > bestCategoryCount)
                return;

            if (groupIndex >= spellGroups.Length)
            {
                var totalCategories = chosenGroupCount + unassignedTargetCount;
                if (totalCategories < bestCategoryCount || (totalCategories == bestCategoryCount && chosenSpellCount > bestNameSpellCount))
                {
                    bestCategoryCount = totalCategories;
                    bestNameSpellCount = chosenSpellCount;
                    bestChosenGroups = new HashSet<string>();
                    for (var i = 0; i < spellGroups.Length; i++)
                        if (groupSelectionMask[i] == '1')
                            bestChosenGroups.Add(spellGroups[i].Name);
                }
                return;
            }

            var group = spellGroups[groupIndex];

            if (group.SpellCount > 1)
            {
                groupSelectionMask[groupIndex] = '1';
                var decrementedTargets = new List<int>();
                foreach (var tIdx in targetsCoveredByGroup[groupIndex])
                {
                    remainingSpellsPerTarget[tIdx]--;
                    if (remainingSpellsPerTarget[tIdx] == 0)
                        unassignedTargetCount--;
                    decrementedTargets.Add(tIdx);
                }

                RecurseSpellGroups(groupIndex + 1, chosenGroupCount + 1, chosenSpellCount + group.SpellCount, unassignedTargetCount);

                // Undo changes
                foreach (var tIdx in decrementedTargets)
                {
                    if (remainingSpellsPerTarget[tIdx] == 0)
                        unassignedTargetCount++;
                    remainingSpellsPerTarget[tIdx]++;
                }

                groupSelectionMask[groupIndex] = '0';
            }

            RecurseSpellGroups(groupIndex + 1, chosenGroupCount, chosenSpellCount, unassignedTargetCount);
        }

        private class SpellGroupInfo
        {
            public string Name;
            public List<SpellViewModel> Spells;
            public int SpellCount;
            public int DistinctTargetCount;
            public bool MatchesPlayerClass;
            public int Impact;
        }
    }
}
