using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class QuakeHandler : BaseHandler
    {
        private readonly PigParseApi pigParseApi;

        public QuakeHandler(BaseHandlerData baseHandlerData, PigParseApi pigParseApi) : base(baseHandlerData)
        {
            logEvents.QuakeEvent += LogEvents_QuakeEvent;
            this.pigParseApi = pigParseApi;
        }

        private void LogEvents_QuakeEvent(object sender, QuakeEvent e)
        {
            var server = activePlayer?.Player?.Server;
            if (server.HasValue)
            {
                pigParseApi.SendQuake(server.Value);
            }
        }

    }
}
