using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.ViewModels
{
    public class BaseWindowViewModel : INotifyPropertyChanged
    {
        private GridLength titlebarSize = new GridLength(20);
        private Thickness borderThickness = new Thickness(1, 0, 1, 1);
        
        public ResizeMode CanResize { get; set; }
        public Visibility ShowTitlebar { get; set; }
        public Thickness BorderThickness { get; set; }
        public GridLength TitleBarHeight { get; set; }
        
        private bool _IsMouseOverTitleArea;
        public bool IsMouseOverTitleArea
        {
            get => _IsMouseOverTitleArea;
            set
            {
                _IsMouseOverTitleArea = value;
                TitleBarHeight = value ? titlebarSize : new GridLength(0);
                OnPropertyChanged(nameof(TitleBarHeight));
                OnPropertyChanged(nameof(IsTitleBarHidden));
                OnPropertyChanged(nameof(EffectiveTitlebarVisibility));
                OnPropertyChanged();
            }
        }

        private bool _IsLocked;
        public bool IsLocked
        {
            get => _IsLocked;
            set
            {
                _IsLocked = value;
                CanResize = value ? ResizeMode.NoResize : ResizeMode.CanResize;
                ShowTitlebar = value ? Visibility.Hidden : Visibility.Visible;
                BorderThickness = value ? new Thickness(0) : borderThickness;
                TitleBarHeight = value ? new GridLength(0) : titlebarSize;

                OnPropertyChanged(nameof(CanResize));
                OnPropertyChanged(nameof(ShowTitlebar));
                OnPropertyChanged(nameof(BorderThickness));
                OnPropertyChanged(nameof(TitleBarHeight));
                OnPropertyChanged(nameof(IsTitleBarHidden));
                OnPropertyChanged(nameof(EffectiveTitlebarVisibility));
                OnPropertyChanged();
            }
        }
        
        public Visibility EffectiveTitlebarVisibility => !IsLocked || IsMouseOverTitleArea ? Visibility.Visible : Visibility.Collapsed;
        public bool IsTitleBarHidden => EffectiveTitlebarVisibility != Visibility.Visible;
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
