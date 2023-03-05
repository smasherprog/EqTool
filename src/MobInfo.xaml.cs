using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        public MobInfo(LogParser logParser, ConLogParse conLogParse, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad)
        {
            this.settings = settings;
            this.toolSettingsLoad = toolSettingsLoad;
            this.logParser = logParser;
            this.conLogParse = conLogParse;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            DataContext = mobInfoViewModel = new ViewModels.MobInfoViewModel();
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.MobWindowState, this);
            SaveState();
            SizeChanged += DPSMeter_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += DPSMeter_LocationChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            logParser.LineReadEvent -= LogParser_LineReadEvent;
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            base.OnClosing(e);
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_LocationChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveState();
        }
        private void SaveState()
        {
            WindowExtensions.SaveWindowState(settings.MobWindowState, this);
            toolSettingsLoad.Save(settings);
        }
        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = conLogParse.ConMatch(e.Line);
            if (!string.IsNullOrWhiteSpace(matched))
            {
                mobInfoViewModel.Results = string.Empty;
                try
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
                catch (System.AggregateException er)
                {
                    if (er.InnerException != null && er.InnerException.GetType() == typeof(HttpRequestException))
                    {
                        var err = er.InnerException as HttpRequestException;
                        if (err.InnerException?.GetType() == typeof(WebException))
                        {
                            var innererr = err.InnerException as WebException;
                            mobInfoViewModel.ErrorResults = innererr.Message;
                        }
                        else
                        {

                            mobInfoViewModel.ErrorResults = err.Message;
                        }
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
            if (settings.MobWindowState == null)
            {
                settings.MobWindowState = new Models.WindowState();
            }
            settings.MobWindowState.Closed = true;
            SaveState();
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
