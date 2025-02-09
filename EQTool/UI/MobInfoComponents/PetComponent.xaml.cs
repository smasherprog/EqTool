using System.Windows.Controls;

namespace EQTool.UI.MobInfoComponents
{
    /// <summary>
    /// Interaction logic for PetComponent.xaml
    /// </summary>
    public partial class PetComponent : UserControl
    {
        public PetComponent(ViewModels.MobInfoComponents.PetViewModel petViewModel)
        {
            DataContext = petViewModel;
            InitializeComponent();
        }

    }
}
