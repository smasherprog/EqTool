using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class PlayerLevelDetectionHandler : BaseHandler
    {
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;

        public PlayerLevelDetectionHandler(
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach,
            EQToolSettings settings,
            EQToolSettingsLoad toolSettingsLoad) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.settings = eQToolSettings;
            this.toolSettingsLoad = toolSettingsLoad;
            this.logEvents.PlayerLevelDetectionEvent += LogEvents_LevelDetectedEvent;
        }

        private void LogEvents_LevelDetectedEvent(object sender, PlayerLevelDetectionEvent e)
        {
            if (activePlayer?.Player != null)
            {
                activePlayer.Player.Level = e.PlayerLevel;
                toolSettingsLoad.Save(settings);
            }
        }
    }
}
