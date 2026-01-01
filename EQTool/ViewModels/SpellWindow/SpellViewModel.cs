using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using EQTool.Services;

namespace EQTool.ViewModels.SpellWindow
{
    [DebuggerDisplay("Group = '{DisplayGroup}' Sorting = '{GroupSorting}' | Spell = '{Id}', Target = '{Target}', Caster = '{Caster}'")]
    public class SpellViewModel : TimerViewModel
    {
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Spell;
        
        public Dictionary<PlayerClasses, int> Classes { get; set; } = new Dictionary<PlayerClasses, int>();
        
        public string Caster { get; set; }
        
        public SpellType SpellType { get; set; }
        
        private SpellBenefitDetriment _BenefitDetrimentFlag = 0;
        public SpellBenefitDetriment BenefitDetriment
        {
            get => _BenefitDetrimentFlag;
            set
            {
                _BenefitDetrimentFlag = value;
                if (_BenefitDetrimentFlag == SpellBenefitDetriment.Beneficial)
                    ProgressBarColor = Brushes.MediumAquamarine;
                else if (_BenefitDetrimentFlag == SpellBenefitDetriment.Detrimental)
                    ProgressBarColor = Brushes.OrangeRed;
                else if (_BenefitDetrimentFlag == SpellBenefitDetriment.Cooldown)
                    ProgressBarColor = Brushes.SkyBlue;
                else
                    ProgressBarColor = Brushes.DarkSeaGreen;
            }
        }
        
        public override string GroupSorting
        {
            get
            {
                var groupName = DisplayGroup;
                if (!IsPlayerTarget || (IsCategorizeById && BenefitDetriment == SpellBenefitDetriment.Detrimental))
                    return SortingPrefixes.Primary + groupName;
                if (groupName == EQSpells.SpaceYou)
                    return SortingPrefixes.Secondary + groupName;
                if (IsCategorizeById && BenefitDetriment != SpellBenefitDetriment.Detrimental)
                    return SortingPrefixes.Tertiary + groupName;
                
                return SortingPrefixes.Quaternary + groupName;
            }
        }
        
        public bool CastOnYou() => Target == EQSpells.You || Target == EQSpells.SpaceYou;
        public virtual bool CastByYou() => Caster == EQSpells.You || Caster == EQSpells.SpaceYou || (SpellType == SpellType.Self && CastOnYou());
        public bool CastByYourClass(PlayerInfo player) => player?.PlayerClass != null && Classes.ContainsKey(player.PlayerClass.Value) || CastByYou();
    }
}
