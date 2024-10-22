using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerLevelDetectionHandler : BaseHandler
    {
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly IAppDispatcher appDispatcher;

        public PlayerLevelDetectionHandler(
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
            this.logEvents.PlayerLevelDetectionEvent += LogEvents_LevelDetectedEvent;
        }

        private void LogEvents_LevelDetectedEvent(object sender, PlayerLevelDetectionEvent e)
        {
            if (activePlayer?.Player != null)
            {
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.Player.Level = e.PlayerLevel;
                });

                toolSettingsLoad.Save(settings);
            }
        }
    }
}
