using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class PlayerLevelDetectionHandler : BaseHandler
    {
        private readonly EQToolSettingsLoad toolSettingsLoad;

        public PlayerLevelDetectionHandler(EQToolSettingsLoad toolSettingsLoad, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.toolSettingsLoad = toolSettingsLoad;
            logEvents.PlayerLevelDetectionEvent += LogEvents_LevelDetectedEvent;
        }

        private void LogEvents_LevelDetectedEvent(object sender, PlayerLevelDetectionEvent e)
        {
            if (activePlayer.Player.Level < e.PlayerLevel)
            {
                if (activePlayer?.Player != null)
                {
                    appDispatcher.DispatchUI(() =>
                    {
                        activePlayer.Player.Level = e.PlayerLevel;
                    });

                    toolSettingsLoad.Save(eQToolSettings);
                }
            }
        }
    }
}
