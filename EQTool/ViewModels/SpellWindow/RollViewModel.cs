﻿namespace EQTool.ViewModels.SpellWindow
{
    public class RollViewModel : TimerViewModel
    {
        private int _Roll = 0;
        public int Roll
        {
            get => _Roll;
            set
            {
                _Roll = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RollText));
            }
        }

        private int _RollOrder = 1;
        public int RollOrder
        {
            get => _RollOrder;
            set
            {
                _RollOrder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RollText));
            }
        }

        private int _MaxRoll = 0;
        public int MaxRoll
        {
            get => _MaxRoll;
            set
            {
                _MaxRoll = value;
                GroupName = $" Random -- {_MaxRoll}";
                OnPropertyChanged();
            }
        }

        public string RollText => $" (#{RollOrder}) Roll --> {Roll}";

        public override string Sorting => " y";

        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Roll;
    }
}
