using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.ViewModels
{
    public class BaseWindowViewModel : INotifyPropertyChanged
    {
        private static GridLength noHeight = new GridLength(0);
        private static GridLength defaultTitlebarSize = new GridLength(20);
        private static Thickness noBorder = new Thickness(0);
        private static Thickness defaultBorderThickness = new Thickness(1, 0, 1, 1);
        private static Thickness defaultResizeBorderThickness = new Thickness(5, 5, 5, 5);
        
        public ResizeMode CanResize { get; set; } = ResizeMode.CanResize;
        public Visibility ShowTitlebar { get; set; } = Visibility.Visible;
        public Thickness BorderThickness { get; set; } = defaultBorderThickness;
        public Thickness ResizeBorderThickness { get; set; } = defaultResizeBorderThickness;
        public GridLength TitleBarHeight { get; set; } = defaultTitlebarSize;
        
        private bool _IsMouseOverTitleArea;
        public bool IsMouseOverTitleArea
        {
            get => _IsMouseOverTitleArea;
            set
            {
                _IsMouseOverTitleArea = value;
                TitleBarHeight = value ? defaultTitlebarSize : noHeight;
                OnPropertyChanged(nameof(TitleBarHeight));
                OnPropertyChanged(nameof(EffectiveTitlebarVisibility));
                OnPropertyChanged(nameof(IsTitleBarHidden));
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
                BorderThickness = value ? noBorder : defaultBorderThickness;
                ResizeBorderThickness = value ? noBorder : defaultResizeBorderThickness;
                TitleBarHeight = value ? noHeight : defaultTitlebarSize;

                OnPropertyChanged(nameof(CanResize));
                OnPropertyChanged(nameof(ShowTitlebar));
                OnPropertyChanged(nameof(BorderThickness));
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(TitleBarHeight));
                OnPropertyChanged(nameof(EffectiveTitlebarVisibility));
                OnPropertyChanged(nameof(IsTitleBarHidden));
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
