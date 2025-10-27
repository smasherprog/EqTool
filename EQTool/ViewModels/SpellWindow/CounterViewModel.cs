using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;

namespace EQTool.ViewModels.SpellWindow
{
    [DebuggerDisplay("Group = {DisplayGroup} Sorting = {GroupSorting} | Count = {Count}, Spell = {Id}, Target = {Target}, Caster = {Caster}")]
    public class CounterViewModel : SpellViewModel
    {
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Counter;
        
        private int _Count = 1;
        public int Count
        {
            get => _Count;
            private set
            {
                _Count = value;
                OnPropertyChanged();
            }
        }
        
        public override string GroupSorting => SortingPrefixes.Primary + DisplayGroup;

        public void AddCount(string caster)
        {
            if (!string.IsNullOrWhiteSpace(caster))
            {
                Casters.Add(caster);
            }
            Count++;
            OnPropertyChanged(nameof(Count));
        }
        
        private HashSet<string> Casters = new HashSet<string>();
        public override bool CastByYou(PlayerInfo player) => Casters.Any(x => x == EQSpells.You || x == EQSpells.SpaceYou || (player != null && x == player.Name));
    }
}
