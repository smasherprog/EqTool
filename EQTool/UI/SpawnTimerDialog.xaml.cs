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
        private readonly SpawnTimerHandler           _spawnTimerHandler;
        private readonly SpawnTimerDialogViewModel   _viewModel;

        // ctor
        public SpawnTimerDialog(SpawnTimerDialogViewModel vm, SpawnTimerHandler handler)
        {
            _viewModel = vm;
            _spawnTimerHandler = handler;

            DataContext = _viewModel;
            _viewModel.SetFrom(_spawnTimerHandler.Model);

            InitializeComponent();
        }


        // function called when OK button is clicked         
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            // capture ViewModel results back into the Model
            _spawnTimerHandler.Model.SetFrom(_viewModel);
        }

        // function called when Cancel button is clicked         
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

    }
}
