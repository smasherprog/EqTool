using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
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
                else
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
            }
        }

        public override string DisplayName => IsCategorizeBySpellName ? Target : Id;
        public override string DisplayGroup => IsCategorizeBySpellName ? Id : Target;
        public override string Sorting => IsCategorizeBySpellName ? Id : Target;

        private bool _IsCategorizeBySpellName;
        public bool IsCategorizeBySpellName
        {
            get => _IsCategorizeBySpellName;
            set
            {
                _IsCategorizeBySpellName = value;
                
                SyncTargetClassString(IsCategorizeBySpellName);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayGroup));
                OnPropertyChanged(nameof(Sorting));
            }
        }
        
        public bool CastOnYou(PlayerInfo player) => Target == EQSpells.You || Target == EQSpells.SpaceYou || (player != null && Target == player.Name);
        public bool CastByYou(PlayerInfo player) => Caster == EQSpells.You || Caster == EQSpells.SpaceYou || (player != null && Caster == player.Name);
    }
}
