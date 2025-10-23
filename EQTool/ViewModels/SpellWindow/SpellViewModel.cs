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
        
        // The "real" Name field for spells. Must use this instead of Name when setting or else our Group by Spell / Group by Target logic will break.
        private string _spellName;
        public string SpellName
        {
            get => _spellName;
            set
            {
                _spellName = value;
                if (string.IsNullOrEmpty(Name))
                {
                    Name = _spellName;
                }
                OnPropertyChanged();
            }
        }
        
        // The "real" GroupName field for spells. Must use this instead of GroupName when setting or else our Group by Spell / Group by Target logic will break.
        private string _targetName;
        public string TargetName
        {
            get => _targetName;
            set
            {
                _targetName = value;
                if (string.IsNullOrEmpty(GroupName))
                {
                    GroupName = _targetName;
                }
                OnPropertyChanged();
            }
        }
        
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
        
        public override string Sorting => IsCategorizeBySpellName ? SpellName : TargetName;

        private bool _IsCategorizeBySpellName;
        public bool IsCategorizeBySpellName
        {
            get => _IsCategorizeBySpellName;
            private set
            {
                _IsCategorizeBySpellName = value;
                
                UpdateCategorization();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sorting));
            }
        }

        private void UpdateCategorization()
        {
            if (IsCategorizeBySpellName)
            {
                Name = TargetName;
                GroupName = SpellName;
            }
            else
            {
                Name = SpellName;
                GroupName = TargetName;
            }
            
            SyncTargetClassString(IsCategorizeBySpellName);
        }
        
        public void UpdateSpellCategorization(EQToolSettings settings)
        {
            IsCategorizeBySpellName = BenefitDetriment == SpellBenefitDetriment.Detrimental
                ? settings.DetrimentalSpellsCategorizedBySpellName
                : settings.BeneficialSpellsCategorizedBySpellName;
        }
        
        public bool CastOnYou(PlayerInfo player) => TargetName == EQSpells.You || TargetName == EQSpells.SpaceYou || (player != null && TargetName == player.Name);
        public bool CastByYou(PlayerInfo player) => Caster == EQSpells.You || Caster == EQSpells.SpaceYou || (player != null && Caster == player.Name);
    }
}
