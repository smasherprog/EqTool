using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
    public class SpellViewModel : TimerViewModel
    {
        public Dictionary<PlayerClasses, int> Classes { get; set; } = new Dictionary<PlayerClasses, int>();

        private bool _ShowOnlyYou = false;
        public bool ShowOnlyYou
        {
            get => _ShowOnlyYou;
            set
            {
                if (_ShowOnlyYou == value)
                {
                    return;
                }
                _ShowOnlyYou = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

        private bool _HideClasses = false;
        public bool HideClasses
        {
            get => _HideClasses;
            set
            {
                if (_HideClasses == value)
                {
                    return;
                }
                _HideClasses = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

        public override Visibility ColumnVisibility
        {
            get
            {
                if (GroupName == CustomTimer.CustomerTime || _Type == SpellTypes.Detrimental)
                {
                    return Visibility.Visible;
                }
                else if (_HideClasses)
                {
                    return Visibility.Collapsed;
                }
                else if (_Type <= 0 || GroupName == EQSpells.SpaceYou)
                {
                    return Visibility.Visible;
                }
                else if (_ShowOnlyYou && GroupName != EQSpells.SpaceYou)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public override string Sorting => GroupName;

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
