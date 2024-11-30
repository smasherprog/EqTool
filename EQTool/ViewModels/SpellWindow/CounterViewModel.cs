namespace EQTool.ViewModels.SpellWindow
{
    public class CounterViewModel : PersistentViewModel
    {
        private int _Count = 0;
        public int Count
        {
            get => _Count;
            set
            {
                _Count = value;
                OnPropertyChanged();
            }
        }

        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Counter;
    }
}
