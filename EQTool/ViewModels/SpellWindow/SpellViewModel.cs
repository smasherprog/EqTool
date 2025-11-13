using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                {
                    ProgressBarColor = Brushes.MediumAquamarine;
                }
                else if (_BenefitDetrimentFlag == SpellBenefitDetriment.Detrimental)
                {
                    ProgressBarColor = Brushes.OrangeRed;
                }
                else if (_BenefitDetrimentFlag == SpellBenefitDetriment.Cooldown)
                {
                    ProgressBarColor = Brushes.SkyBlue;
                }
                else
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
            }
        }
        
        public override string GroupSorting
        {
            get
            {
                var groupName = DisplayGroup;
                if ((groupName.StartsWith(" ") && groupName != EQSpells.SpaceYou) || (IsCategorizeById && BenefitDetriment == SpellBenefitDetriment.Detrimental))
                {
                    return SortingPrefixes.Primary + groupName;
                }
                if (groupName == EQSpells.SpaceYou)
                {
                    return SortingPrefixes.Secondary + groupName;
                }
                if (IsCategorizeById && BenefitDetriment == SpellBenefitDetriment.Cooldown)
                {
                    return SortingPrefixes.Tertiary + groupName;
                }
                if (IsCategorizeById && BenefitDetriment == SpellBenefitDetriment.Beneficial)
                {
                    return SortingPrefixes.Quaternary + groupName;
                }
                
                return SortingPrefixes.Tertiary + groupName;
            }
        }
        
        public bool CastOnYou(PlayerInfo player) => Target == EQSpells.You || Target == EQSpells.SpaceYou || (player != null && Target == player.Name);
        public virtual bool CastByYou(PlayerInfo player) => Caster == EQSpells.You || Caster == EQSpells.SpaceYou || (player != null && Caster == player.Name);
        public bool CastByYourClass(PlayerInfo player) => CastByYou(player) || (player.PlayerClass.HasValue && Classes.ContainsKey(player.PlayerClass.Value));
    }
}
