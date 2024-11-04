using EQTool.Services.Handlers;
using EQTool.ViewModels;
using System.Windows;

namespace EQTool.UI
{
    /// <summary>
    /// Interaction logic for SpawnTimerDialogForms.xaml
    /// </summary>
    public partial class SpawnTimerDialog : Window
    {
        private SpawnTimerDialogViewModel viewModel;
        public SpawnTimerDialogViewModel ViewModel { get { return viewModel; } set { viewModel = value;} }

        public SpawnTimerDialog(SpawnTimerDialogViewModel vm)
        {
            viewModel = vm;
            DataContext = viewModel;
            InitializeComponent();
        }


        // function called when OK button is clicked         
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //viewModel.DoSomething_DontTryTopassData_ItShouldBeInTheeViewModelAllready();
            
            // capture ViewModel results back into the Model
            // todo - design question = should this happen here, or at the location that launched this dialog?
            DialogResult = true;

        }

        // function called when Cancel button is clicked         
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
