using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class YouZonedHandler : BaseHandler
    {
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly IAppDispatcher appDispatcher;

        public YouZonedHandler(
            IAppDispatcher appDispatcher,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach,
            EQToolSettings settings,
            EQToolSettingsLoad toolSettingsLoad) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.settings = eQToolSettings;
            this.toolSettingsLoad = toolSettingsLoad;
            this.logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
        }

        private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        {
            var currentzone = activePlayer?.Player?.Zone;
            if (currentzone != e.ShortName)
            {
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.Player.Zone = e.ShortName;
                });

                toolSettingsLoad.Save(settings);
                var text = $"You zoned into {e.LongName}";
                if (activePlayer?.Player?.EnteringZoneAudio == true)
                {
                    textToSpeach.Say(text);
                }

                var doAlert = activePlayer?.Player?.EnteringZoneOverlay ?? false;
                if (doAlert)
                {
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(3000);
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                    });
                }
            }
        }
    }
}
