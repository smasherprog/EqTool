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
        public SpellType SpellType { get; set; }
        private SpellTypes _Type = 0;
        public SpellTypes Type
        {
            get => _Type;
            set
            {
                _Type = value;
                if (_Type == SpellTypes.Beneficial)
                {
                    ProgressBarColor = Brushes.MediumAquamarine;
                }
                else if (_Type == SpellTypes.Detrimental)
                {
                    ProgressBarColor = Brushes.OrangeRed;
                }
                else if (_Type >= SpellTypes.Other)
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
                else
                {
                    ProgressBarColor = Brushes.DarkSeaGreen;
                }
            }
        }
    }
}
