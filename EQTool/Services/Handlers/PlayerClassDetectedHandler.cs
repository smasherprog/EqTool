using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class PlayerClassDetectedHandler : BaseHandler
    {
        private readonly EQToolSettingsLoad toolSettingsLoad;

        public PlayerClassDetectedHandler(EQToolSettingsLoad toolSettingsLoad, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.toolSettingsLoad = toolSettingsLoad;
            logEvents.ClassDetectedEvent += LogEvents_ClassDetectedEvent;
        }

        private void LogEvents_ClassDetectedEvent(object sender, ClassDetectedEvent e)
        {
            if (activePlayer?.Player != null && activePlayer.Player.PlayerClass != e.PlayerClass)
            {
                appDispatcher.DispatchUI(() =>
                {
                    activePlayer.Player.PlayerClass = e.PlayerClass;
                    toolSettingsLoad.Save(eQToolSettings);
                }); 
            }
        }
    }
}
