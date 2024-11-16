namespace EQTool.ViewModels.SpellWindow
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
            }
        }
        public override string Sorting => " y";

        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Roll;
    }
}
