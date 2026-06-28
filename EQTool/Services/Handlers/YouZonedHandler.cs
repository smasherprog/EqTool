using EQTool.Models;

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
            }
        }
    }
}
