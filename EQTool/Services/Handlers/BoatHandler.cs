using EQTool.Models;
using EQToolShared.APIModels.BoatControllerModels;
using System.Threading.Tasks;

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
            var s = activePlayer.Player?.Server;
            if (s.HasValue)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    pigParseApi.SendBoatData(new BoatActivityRequest
                    {
                        Boat = e.Boat,
                        Server = s.Value,
                        StartPoint = e.StartPoint
                    });
                });
            }
        }
    }
}
