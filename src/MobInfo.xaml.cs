using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MobInfo.xaml
    /// </summary>
    public partial class MobInfo : BaseSaveStateWindow
    {
        private readonly LogParser logParser;
        private readonly ViewModels.MobInfoViewModel mobInfoViewModel;
        private readonly WikiApi wikiApi;
        private readonly PigParseApi pigParseApi;
        private readonly ActivePlayer activePlayer;
        public MobInfo(ActivePlayer activePlayer, PigParseApi pigParseApi, WikiApi wikiApi, LogParser logParser, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService)
            : base(settings.MobWindowState, toolSettingsLoad, settings)
        {
            loggingService.Log(string.Empty, EventType.OpenMobInfo, activePlayer?.Player?.Server);
            this.activePlayer = activePlayer;
            this.pigParseApi = pigParseApi;
            this.wikiApi = wikiApi;
            this.logParser = logParser;
            DataContext = mobInfoViewModel = new ViewModels.MobInfoViewModel();
            InitializeComponent();
            base.Init();
            this.logParser.ConEvent += LogParser_ConEvent;
        }

        private void LogParser_ConEvent(object sender, LogParser.ConEventArgs e)
        {
            try
            {
                if (e.Name != mobInfoViewModel.Name)
                {
                    mobInfoViewModel.Results = wikiApi.GetData(e.Name);
                    var items = mobInfoViewModel.KnownLoot.Where(a => a.HasUrl == Visibility.Visible).Select(a => a.Name?.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
                    if (activePlayer?.Player?.Server != null && items.Any())
                    {
                        var itemprices = pigParseApi.GetData(items, activePlayer.Player.Server.Value);
                        foreach (var item in itemprices)
                        {
                            var loot = mobInfoViewModel.KnownLoot.FirstOrDefault(a => a.Name.Equals(item.ItemName, StringComparison.OrdinalIgnoreCase));
                            if (loot != null)
                            {
                                loot.Price = item.TotalWTSLast6MonthsAverage.ToString();
                                loot.PriceUrl = $"https://pigparse.azurewebsites.net/ItemDetails/{item.EQitemId}";
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mobInfoViewModel.ErrorResults = ex.Message;
                if (!mobInfoViewModel.ErrorResults.Contains("The underlying connection was closed:"))
                {
                    mobInfoViewModel.ErrorResults = "The server is down. Try again";
                    App.LogUnhandledException(ex, $"LogParser_ConEvent {e.Name}", activePlayer?.Player?.Server);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (logParser != null)
            {
                logParser.ConEvent -= LogParser_ConEvent;
            }
            base.OnClosing(e);
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
