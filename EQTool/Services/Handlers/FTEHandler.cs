using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class FTEHandler : BaseHandler
    {
        private readonly PigParseApi pigParseApi;
        public FTEHandler(LogEvents logEvents, PigParseApi pigParseApi, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.pigParseApi = pigParseApi;
            this.logEvents.FTEEvent += LogParser_FTEEvent;
        }

        private void LogParser_FTEEvent(object sender, FTEEvent e)
        {
            if (activePlayer?.Player?.FTEAudio == true)
            {
                textToSpeach.Say($"{e.FTEPerson} F T E {e.NPCName}");
            }
            var doAlert = activePlayer?.Player?.FTEOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var fteperson = pigParseApi.GetPlayerData(e.FTEPerson, activePlayer.Player.Server.Value);
                    var text = $"{e.FTEPerson} FTE {e.NPCName}";
                    if (fteperson != null)
                    {
                        text = $"{fteperson.Name} <{fteperson.GuildName}> FTE {e.NPCName}";
                    }

                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = true });
                });
            }
        }
    }
}
