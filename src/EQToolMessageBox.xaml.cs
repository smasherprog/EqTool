using System.Windows;

namespace EQTool
{
    public partial class EQToolMessageBox : Window
    {
        public EQToolMessageBox(string title, string body)
        {
            Topmost = true;
            InitializeComponent();
            EQTitle.Text = title;
            EQBody.Text = body;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
