using EQTool.Models;
using EQTool.ViewModels;

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
            if (activePlayer?.Player != null)
            {
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.Player.Zone = e.ShortName;
                });

                toolSettingsLoad.Save(settings);
            }
        }
    }
}
