using EQTool.ViewModels;
using System.Windows;

namespace EQTool.UI
{
    /// <summary>
    /// Interaction logic for SpawnTimerDialogForms.xaml
    /// </summary>
    public partial class SpawnTimerDialog : Window
    {
        private readonly SpawnTimerDialogViewModel ViewModels = new SpawnTimerDialogViewModel();
        public SpawnTimerDialog()
        { 
            this.DataContext = ViewModels;  
            InitializeComponent();
        }
    }
}
