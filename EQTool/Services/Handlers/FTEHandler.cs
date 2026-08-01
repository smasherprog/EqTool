using EQTool.Models;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    // Announces "<player> FTE <npc>" (overlay text and/or TTS, per the player's FTE settings).
    // The FTE %-rule countdown timers (96%/97%/Lodizal) that used to live here are now built-in
    // triggers - see BuiltInTriggers.CreateFTE97Rule and friends.
    public class FTEHandler : BaseHandler
    {
        private readonly PigParseApi pigParseApi;

        public FTEHandler(PigParseApi pigParseApi, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.pigParseApi = pigParseApi;
            logEvents.FTEEvent += LogParser_FTEEvent;
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
