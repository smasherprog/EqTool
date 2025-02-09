using EQTool.ViewModels.MobInfoComponents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EQTool.UI.MobInfoComponents
{
    /// <summary>
    /// Interaction logic for MobComponent.xaml
    /// </summary>
    public partial class MobComponent : UserControl
    {
        private readonly MobInfoViewModel mobInfoViewModel;
        public MobComponent(MobInfoViewModel mobInfoViewModel)
        {
            DataContext = this.mobInfoViewModel = mobInfoViewModel;
            InitializeComponent();

        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigatebutton(object sender, RoutedEventArgs args)
        {
            _ = Process.Start(new ProcessStartInfo(mobInfoViewModel.Url));
        }
    }
}
