using EQTool.ViewModels;
using System.Windows;

namespace EQTool.UI
{
    /// <summary>
    /// Interaction logic for SpawnTimerDialogForms.xaml
    /// </summary>
    public partial class SpawnTimerDialog : Window
    {
        private readonly SpawnTimerDialogViewModel ViewModel;
        public SpawnTimerDialog(SpawnTimerDialogViewModel viewmodel)
        {
            this.DataContext = ViewModel = viewmodel;
            InitializeComponent();
        }
    }
}
