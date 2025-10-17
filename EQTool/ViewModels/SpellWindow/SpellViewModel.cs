using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
    public class SpellViewModel : TimerViewModel
    {
        public Dictionary<PlayerClasses, int> Classes { get; set; } = new Dictionary<PlayerClasses, int>();
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Spell;
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
                else
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
            }
        }
        
        public bool CastByYou(PlayerInfo player) => Caster == EQSpells.You || Caster == EQSpells.SpaceYou || Caster == player.Name;
    }
}
