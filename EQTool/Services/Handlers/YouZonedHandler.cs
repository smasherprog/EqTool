using EQTool.Models;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class YouZonedHandler : BaseHandler
    {
        private readonly EQToolSettingsLoad toolSettingsLoad;

        public YouZonedHandler(EQToolSettingsLoad toolSettingsLoad, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.toolSettingsLoad = toolSettingsLoad;
            logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
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

                toolSettingsLoad.Save(eQToolSettings);
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
