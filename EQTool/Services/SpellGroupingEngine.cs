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
        private readonly Dictionary<string, List<SpellViewModel>> activeSpellIds = new Dictionary<string, List<SpellViewModel>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<SpellViewModel>> spellsByTarget = new Dictionary<string, List<SpellViewModel>>(StringComparer.Ordinal);

        public SpellGroupingEngine(PlayerInfo activePlayer, EQToolSettings settings)
        {
            this.activePlayer = activePlayer;
            this.settings = settings;
        }
        
        public void Recategorize(IEnumerable<SpellViewModel> deltaSpells = null)
        {
            if (deltaSpells != null && deltaSpells.Any())
                deltaSpells = AllSpellsRelatedToTargetOrId(deltaSpells);
            
            var spellsToGroup = GetSpellsToEvaluate(deltaSpells ?? allSpells);
            if (!spellsToGroup.Any())
                return;

            RunFullCategorization(spellsToGroup);
        }

        private IEnumerable<SpellViewModel> AllSpellsRelatedToTargetOrId(IEnumerable<SpellViewModel> sourceSpells)
        {
            var relevantTargets = sourceSpells.Select(s => s.Target).ToHashSet();
            var relevantNames = sourceSpells.Select(s => s.Id).ToHashSet();

            var related = new HashSet<SpellViewModel>();
            foreach (var target in relevantTargets)
            {
                if (!spellsByTarget.TryGetValue(target, out var associated))
                    continue;

                foreach (var s in associated)
                {
                    relevantNames.Add(s.Id);    // Every spell on every related target has to be added to the pile.
                    related.Add(s);
                }
            }

            foreach (var name in relevantNames)
            {
                if (!activeSpellIds.TryGetValue(name, out var associated))
                    continue;

                foreach (var s in associated)
                    related.Add(s);
            }
                
            return related.ToList();
        }

        public void AddSpells(IEnumerable<SpellViewModel> newSpells)
        {
            foreach (var spell in newSpells)
            {
                allSpells.Add(spell);
                activeSpellIds.SafelyAdd(spell.Id, spell);
                spellsByTarget.SafelyAdd(spell.Target, spell);
            }
        }

        public void RemoveSpells(IEnumerable<SpellViewModel> removedSpells)
        {
            foreach (var spell in removedSpells)
            {
                allSpells.Remove(spell);
                activeSpellIds.SafelyRemove(spell.Id, spell);
                spellsByTarget.SafelyRemove(spell.Target, spell);
            }
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
        
        private IEnumerable<SpellViewModel> GetSpellsToEvaluate(IEnumerable<SpellViewModel> baselineSpells)
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

        // -----------------------
        // Branch-and-bound algorithm. Determine what should be grouped by target and what should be grouped by Id.
        // -----------------------
        private void RunFullCategorization(IEnumerable<SpellViewModel> visibleSpells)
        {
            var context = new BatchAndBoundContext(activePlayer, visibleSpells);
            
            var totalUnassignedTargets = context.RemainingSpellsPerTarget.Count(c => c > 0);
            ComputeSpellGroups(context, 0, 0, 0, totalUnassignedTargets);

            // Apply our chosen state
            foreach (var spell in visibleSpells)
                spell.IsCategorizeById = context.BestChosenGroups.Contains(spell.Id);
        }

        private static void ComputeSpellGroups(BatchAndBoundContext context, int grpIndex, int chosenGroupCount, int chosenSpellCount, int unassignedTargetCount)
        {
            var stateKey = grpIndex + "|" + new string(context.GroupSelectionMask);
            if (!context.VisitedStates.Add(stateKey))
                return;

            var lowerBoundCategoryCount = chosenGroupCount + unassignedTargetCount;
            if (lowerBoundCategoryCount > context.BestCategoryCount)
                return;

            if (grpIndex >= context.GroupInfo.Length)
            {
                var totalCategories = chosenGroupCount + unassignedTargetCount;
                if (totalCategories < context.BestCategoryCount || (totalCategories == context.BestCategoryCount && chosenSpellCount > context.BestNameSpellCount))
                {
                    context.BestCategoryCount = totalCategories;
                    context.BestNameSpellCount = chosenSpellCount;
                    context.BestChosenGroups.Clear();
                    for (var i = 0; i < context.GroupInfo.Length; i++)
                    {
                        if (context.GroupSelectionMask[i] == '1')
                            context.BestChosenGroups.Add(context.GroupInfo[i].Name);
                    }
                }
                return;
            }

            var group = context.GroupInfo[grpIndex];
            if (group.SpellCount > 1)
            {
                context.GroupSelectionMask[grpIndex] = '1';
                var decrementedTargets = new List<int>();
                foreach (var tIdx in context.TargetsCoveredByGroup[grpIndex])
                {
                    context.RemainingSpellsPerTarget[tIdx]--;
                    if (context.RemainingSpellsPerTarget[tIdx] == 0)
                        unassignedTargetCount--;
                    
                    decrementedTargets.Add(tIdx);
                }

                ComputeSpellGroups(context, grpIndex + 1, chosenGroupCount + 1, chosenSpellCount + group.SpellCount, unassignedTargetCount);

                // Undo changes
                foreach (var tIdx in decrementedTargets)
                {
                    if (context.RemainingSpellsPerTarget[tIdx] == 0)
                        unassignedTargetCount++;
                    
                    context.RemainingSpellsPerTarget[tIdx]++;
                }

                context.GroupSelectionMask[grpIndex] = '0';
            }

            ComputeSpellGroups(context, grpIndex + 1, chosenGroupCount, chosenSpellCount, unassignedTargetCount);
        }

        private class BatchAndBoundContext
        {
            public SpellGroupInfo[] GroupInfo { get; }
            public List<int>[] TargetsCoveredByGroup { get; }
            public int[] RemainingSpellsPerTarget { get; }
            public string[] AllTargets { get; }
            public char[] GroupSelectionMask { get; }
            public HashSet<string> VisitedStates { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> BestChosenGroups { get; } = new HashSet<string>();
            
            public int BestCategoryCount { get; set; } = int.MaxValue;
            public int BestNameSpellCount { get; set; } = -1;

            public BatchAndBoundContext(PlayerInfo activePlayer, IEnumerable<SpellViewModel> visibleSpells)
            {
                var spellsById = visibleSpells.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.ToList());

                GroupInfo = spellsById.Values
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

                AllTargets = visibleSpells.Select(s => s.Target).Distinct().ToArray();
                var targetIndexLookup = AllTargets.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i, StringComparer.Ordinal);

                TargetsCoveredByGroup = new List<int>[GroupInfo.Length];
                RemainingSpellsPerTarget = new int[AllTargets.Length];

                for (var i = 0; i < GroupInfo.Length; i++)
                {
                    var targetIndices = GroupInfo[i].Spells.Select(s => targetIndexLookup[s.Target]).Distinct().ToList();
                    TargetsCoveredByGroup[i] = targetIndices;

                    foreach (var idx in targetIndices)
                        RemainingSpellsPerTarget[idx]++;
                }

                BestChosenGroups.Clear();
                VisitedStates.Clear();
                GroupSelectionMask = Enumerable.Repeat('0', GroupInfo.Length).ToArray();
            }
        }

        private class SpellGroupInfo
        {
            public string Name { get; set; }
            public List<SpellViewModel> Spells { get; set; }
            public int SpellCount { get; set; }
            public int DistinctTargetCount { get; set; }
            public bool MatchesPlayerClass { get; set; }
            public int Impact { get; set; }
        }
    }
}
