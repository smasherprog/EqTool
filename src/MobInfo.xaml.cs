using EQTool.Services;
using EQTool.Services.Spells.Log;
using System.Diagnostics;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MobInfo.xaml
    /// </summary>
    public partial class MobInfo : Window
    {
        private readonly LogParser logParser;
        private readonly ConLogParse conLogParse;
        private readonly ViewModels.MobInfoViewModel mobInfoViewModel;

        public MobInfo(LogParser logParser, ConLogParse conLogParse)
        {
            this.logParser = logParser;
            this.conLogParse = conLogParse;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            DataContext = mobInfoViewModel = new ViewModels.MobInfoViewModel();
            Topmost = true;
            InitializeComponent();

        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = conLogParse.ConMatch(e.Line);
            if (!string.IsNullOrWhiteSpace(matched))
            {
                var name = HttpUtility.UrlEncode(matched.Trim().Replace(' ', '_'));
                var url = $"https://wiki.project1999.com/{name}?action=raw";
                var res = App.httpclient.GetAsync(url).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    if (response.StartsWith("#REDIRECT"))
                    {
                        name = response.Replace("#REDIRECT", string.Empty)?.Replace("[[:", string.Empty)?.Replace("[[", string.Empty)?.Replace("]]", string.Empty)?.Trim();
                        name = HttpUtility.UrlEncode(name.Replace(' ', '_'));
                        url = $"https://wiki.project1999.com/{name}?action=raw";
                        res = App.httpclient.GetAsync(url).Result;
                        if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            mobInfoViewModel.Results = res.Content.ReadAsStringAsync().Result;
                        }
                    }
                    else
                    {
                        mobInfoViewModel.Results = response;
                    }
                }

            }
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


        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void opendps(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenDPSWindow();
        }

        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWindow();
        }

        private void openmap(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMapWindow();
        }

        private void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWindow();
        }
    }
}
