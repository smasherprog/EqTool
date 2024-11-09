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
        private SpawnTimerHandler spawnTimerHandler;
        private SpawnTimerDialogViewModel viewModel;
        public SpawnTimerDialogViewModel ViewModel { get { return viewModel; } set { viewModel = value;} }

        // ctor
        public SpawnTimerDialog(SpawnTimerDialogViewModel vm, SpawnTimerHandler handler)
        {
            InitializeComponent();

            spawnTimerHandler = handler;
            viewModel = vm;
            DataContext = viewModel;
            viewModel.SetFrom(spawnTimerHandler.Trigger);
        }


        // function called when OK button is clicked         
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            // capture ViewModel results back into the Model
            spawnTimerHandler.Trigger.SetFrom(viewModel);
        }

        // function called when Cancel button is clicked         
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
