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

        private readonly List<SpellViewModel> allSpells = new List<SpellViewModel>();
        private readonly Dictionary<string, List<SpellViewModel>> spellsById = new Dictionary<string, List<SpellViewModel>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<SpellViewModel>> spellsByTarget = new Dictionary<string, List<SpellViewModel>>(StringComparer.Ordinal);

        public SpellGroupingEngine(PlayerInfo activePlayer, EQToolSettings settings)
        {
            this.activePlayer = activePlayer;
            this.settings = settings;
        }
        
        public void Recategorize(IEnumerable<SpellViewModel> changedSpells = null)
        {
            if (changedSpells != null && changedSpells.Any())
                changedSpells = GetConnectedSpells(changedSpells);
            
            var visibleForEval = GetSpellsNeedingAutomaticGrouping(changedSpells ?? allSpells);
            if (!visibleForEval.Any())
                return;

            PerformAutomaticGrouping(visibleForEval);
        }

        private IEnumerable<SpellViewModel> GetConnectedSpells(IEnumerable<SpellViewModel> changedSpells)
        {
            var seed = changedSpells as IList<SpellViewModel> ?? changedSpells.ToList();
            if (!seed.Any())
                return Enumerable.Empty<SpellViewModel>();
            
            var visitedIds = new HashSet<string>(StringComparer.Ordinal);
            var visitedTargets = new HashSet<string>(StringComparer.Ordinal);
            var connected = new HashSet<SpellViewModel>();

            var idQueue = new Queue<string>();
            var targetQueue = new Queue<string>();

            foreach (var spell in seed)
            {
                connected.Add(spell);

                if (visitedIds.Add(spell.Id))
                    idQueue.Enqueue(spell.Id);

                if (visitedTargets.Add(spell.Target))
                    targetQueue.Enqueue(spell.Target);
            }

            while (idQueue.Count > 0 || targetQueue.Count > 0)
            {
                while (targetQueue.Count > 0)
                {
                    var target = targetQueue.Dequeue();
                    if (!spellsByTarget.TryGetValue(target, out var spellsOnTarget))
                        continue;

                    foreach (var spell in spellsOnTarget.Where(spell => connected.Add(spell)))
                    {
                        if (visitedIds.Add(spell.Id))
                            idQueue.Enqueue(spell.Id);

                        if (visitedTargets.Add(spell.Target))
                            targetQueue.Enqueue(spell.Target);
                    }
                }

                while (idQueue.Count > 0)
                {
                    var id = idQueue.Dequeue();
                    if (!spellsById.TryGetValue(id, out var spellsWithId))
                        continue;

                    foreach (var spell in spellsWithId.Where(spell => connected.Add(spell)))
                    {
                        if (visitedTargets.Add(spell.Target))
                            targetQueue.Enqueue(spell.Target);

                        if (visitedIds.Add(spell.Id))
                            idQueue.Enqueue(spell.Id);
                    }
                }
            }

            return connected.ToList();
        }

        public void AddSpells(IEnumerable<SpellViewModel> spells)
        {
            foreach (var spell in spells)
            {
                allSpells.Add(spell);
                spellsById.SafelyAdd(spell.Id, spell);
                spellsByTarget.SafelyAdd(spell.Target, spell);
            }
        }

        public void RemoveSpells(IEnumerable<SpellViewModel> spells)
        {
            foreach (var spell in spells)
            {
                allSpells.Remove(spell);
                spellsById.SafelyRemove(spell.Id, spell);
                spellsByTarget.SafelyRemove(spell.Target, spell);
            }
        }

        public void ApplyNonAutomaticGroupingRule(SpellViewModel spell)
        {
            var mode = GetConfiguredGroupingMode(spell);

            if (mode == SpellGroupingType.ByTarget)
                spell.IsCategorizeById = false;
            else if (mode == SpellGroupingType.BySpell)
                spell.IsCategorizeById = true;
            else if (mode == SpellGroupingType.BySpellExceptYou)
                spell.IsCategorizeById = !spell.CastOnYou();
            
            // Automatic grouping handled elsewhere due to it needed to evaluate the whole list at once.
        }
        
        private IEnumerable<SpellViewModel> GetSpellsNeedingAutomaticGrouping(IEnumerable<SpellViewModel> spells)
        {
            var playerMode = settings.PlayerSpellGroupingType == SpellGroupingType.Automatic;
            var npcMode = settings.NpcSpellGroupingType == SpellGroupingType.Automatic;

            if (!playerMode && !npcMode)
                return Enumerable.Empty<SpellViewModel>();

            return spells.Where(s => s.ColumnVisibility == Visibility.Visible && ((s.IsPlayerTarget && playerMode) || (!s.IsPlayerTarget && npcMode)));
        }

        private SpellGroupingType GetConfiguredGroupingMode(SpellViewModel spell)
            => spell.IsPlayerTarget
                ? settings.PlayerSpellGroupingType
                : settings.NpcSpellGroupingType;

        // -----------------------
        // Branch-and-bound algorithm. Determine what should be grouped by target and what should be grouped by Id.
        // -----------------------
        private void PerformAutomaticGrouping(IEnumerable<SpellViewModel> visibleSpells)
        {
            var context = new BranchAndBoundContext(activePlayer, visibleSpells);
            
            var unassignedTargetCount = context.RemainingSpellsPerTarget.Count(x => x > 0);
            ComputeOptimalGroups(context, 0, 0, 0, unassignedTargetCount);

            // Apply our chosen state
            foreach (var spell in visibleSpells)
                spell.IsCategorizeById = context.SelectedIdGroups.Contains(spell.Id);
        }

        private static void ComputeOptimalGroups(
            BranchAndBoundContext context,
            int groupIndex,
            int selectedGroupCount,
            int totalSpellsSelectedById,
            int remainingTargets)
        {
            var stateKey = groupIndex + "|" + new string(context.SelectionMask);
            if (!context.Visited.Add(stateKey))
                return;

            var lowerBound = selectedGroupCount + remainingTargets;
            if (lowerBound > context.BestCategoryCount)
                return;

            if (groupIndex >= context.Groups.Length)
            {
                var finalCategories = selectedGroupCount + remainingTargets;
                if (finalCategories < context.BestCategoryCount ||
                    (finalCategories == context.BestCategoryCount && totalSpellsSelectedById > context.BestNameSpellCount))
                {
                    context.BestCategoryCount = finalCategories;
                    context.BestNameSpellCount = totalSpellsSelectedById;
                    context.SelectedIdGroups.Clear();

                    for (var i = 0; i < context.Groups.Length; i++)
                        if (context.SelectionMask[i] == '1')
                            context.SelectedIdGroups.Add(context.Groups[i].GroupId);
                }
                return;
            }

            var group = context.Groups[groupIndex];
            if (group.SpellCount > 1)
            {
                context.SelectionMask[groupIndex] = '1';
                var touchedTargets = new List<int>();

                foreach (var t in context.TargetIndicesPerGroup[groupIndex])
                {
                    context.RemainingSpellsPerTarget[t]--;
                    if (context.RemainingSpellsPerTarget[t] == 0)
                        remainingTargets--;

                    touchedTargets.Add(t);
                }

                ComputeOptimalGroups(
                    context,
                    groupIndex + 1,
                    selectedGroupCount + 1,
                    totalSpellsSelectedById + group.SpellCount,
                    remainingTargets);

                foreach (var t in touchedTargets)
                {
                    if (context.RemainingSpellsPerTarget[t] == 0)
                        remainingTargets++;
                    context.RemainingSpellsPerTarget[t]++;
                }

                context.SelectionMask[groupIndex] = '0';
            }

            ComputeOptimalGroups(context, groupIndex + 1, selectedGroupCount, totalSpellsSelectedById, remainingTargets);
        }

        private class BranchAndBoundContext
        {
            public SpellIdGroupInfo[] Groups { get; }
            public List<int>[] TargetIndicesPerGroup { get; }
            public int[] RemainingSpellsPerTarget { get; }
            public string[] TargetList { get; }
            public char[] SelectionMask { get; }
            public HashSet<string> Visited { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> SelectedIdGroups { get; } = new HashSet<string>();
            
            public int BestCategoryCount { get; set; } = int.MaxValue;
            public int BestNameSpellCount { get; set; } = -1;

            public BranchAndBoundContext(PlayerInfo activePlayer, IEnumerable<SpellViewModel> visibleSpells)
            {
                var groupsById = visibleSpells
                    .GroupBy(s => s.Id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Groups = groupsById.Values
                    .Select(list =>
                    {
                        var info = new SpellIdGroupInfo
                        {
                            GroupId = list[0].Id,
                            Spells = list,
                            SpellCount = list.Count,
                            DistinctTargetCount = list.Select(s => s.Target).Distinct().Count(),
                            CastableByPlayerClass = list.Any(s => s.CastByYourClass(activePlayer))
                        };
                        info.PriorityScore = info.SpellCount - info.DistinctTargetCount;
                        return info;
                    })
                    .OrderByDescending(g => g.CastableByPlayerClass)
                    .ThenByDescending(g => g.PriorityScore)
                    .ThenByDescending(g => g.SpellCount)
                    .ToArray();

                TargetList = visibleSpells.Select(s => s.Target).Distinct().ToArray();
                var targetIndex = TargetList
                    .Select((t, i) => (t, i))
                    .ToDictionary(x => x.t, x => x.i, StringComparer.Ordinal);

                TargetIndicesPerGroup = new List<int>[Groups.Length];
                RemainingSpellsPerTarget = new int[TargetList.Length];

                for (var i = 0; i < Groups.Length; i++)
                {
                    var indices = Groups[i].Spells
                        .Select(s => targetIndex[s.Target])
                        .Distinct()
                        .ToList();

                    TargetIndicesPerGroup[i] = indices;

                    foreach (var idx in indices)
                        RemainingSpellsPerTarget[idx]++;
                }
                
                SelectionMask = Enumerable.Repeat('0', Groups.Length).ToArray();
            }
        }

        private class SpellIdGroupInfo
        {
            public string GroupId { get; set; }
            public List<SpellViewModel> Spells { get; set; }
            public int SpellCount { get; set; }
            public int DistinctTargetCount { get; set; }
            public bool CastableByPlayerClass { get; set; }
            public int PriorityScore { get; set; }
        }
    }
}
