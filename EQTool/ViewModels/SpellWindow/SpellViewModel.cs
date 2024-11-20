using EQTool.Models;
using EQToolShared.Enums;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
    public class SpellViewModel : TimerViewModel
    {
        public bool GuessedSpell { get; set; }

        public Dictionary<PlayerClasses, int> Classes { get; set; } = new Dictionary<PlayerClasses, int>();

        private bool _HideGuesses = true;

        public bool HideGuesses
        {
            get => _HideGuesses;
            set
            {
                if (_HideGuesses == value)
                {
                    return;
                }
                _HideGuesses = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColumnVisibility));
            }
        }

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
                if (GroupName == CustomTimer.CustomerTime || _SpellType == SpellTypes.Detrimental)
                {
                    return Visibility.Visible;
                }
                else if (_HideGuesses && GuessedSpell)
                {
                    return Visibility.Collapsed;
                }
                else if (_HideClasses)
                {
                    return Visibility.Collapsed;
                }
                else if (_SpellType <= 0 || GroupName == EQSpells.SpaceYou)
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
        private SpellTypes _SpellType = 0;
        public SpellTypes SpellType
        {
            get => _SpellType;
            set
            {
                _SpellType = value;
                if (_SpellType == SpellTypes.Beneficial)
                {
                    ProgressBarColor = Brushes.MediumAquamarine;
                }
                else if (_SpellType == SpellTypes.Detrimental)
                {
                    ProgressBarColor = Brushes.OrangeRed;
                }
                else if (_SpellType >= SpellTypes.Other)
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
