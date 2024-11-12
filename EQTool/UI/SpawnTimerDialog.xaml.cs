using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQTool.UI;
using System.Windows;

namespace EQTool.UI
{
    /// <summary>
    /// Interaction logic for SpawnTimerDialogForms.xaml
    /// </summary>
    public partial class SpawnTimerDialog : Window
    {
        private SpawnTimerHandler           _spawnTimerHandler;
        private SpawnTimerDialogViewModel   _viewModel;

        //public SpawnTimerDialogViewModel ViewModel { get { return _viewModel; } set { _viewModel = value;} }

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
