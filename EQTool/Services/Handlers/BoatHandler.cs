using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.APIModels.BoatControllerModels;

namespace EQTool.Services.Handlers
{
    public class BoatHandler : BaseHandler
    {
        private readonly PigParseApi pigParseApi;

        public BoatHandler(PigParseApi pigParseApi, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.pigParseApi = pigParseApi;
            logEvents.BoatEvent += LogEvents_BoatEvent;
        }

        private void LogEvents_BoatEvent(object sender, BoatEvent e)
        {
            var s = this.activePlayer.Player?.Server;
            if (s.HasValue)
            {
                pigParseApi.SendBoatData(new BoatActivityRequest
                {
                    Server = s.Value,
                    Zone = e.ZoneName
                });
            }
        }
    }
}
