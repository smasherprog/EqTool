using EQTool.Models;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class GroupInviteHandler : BaseHandler
    {
        public GroupInviteHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.GroupInviteEvent += LogParser_GroupInviteEvent;
        }

        private void LogParser_GroupInviteEvent(object sender, GroupInviteEvent e)
        {
            if (activePlayer?.Player?.GroupInviteAudio == true)
            {
                textToSpeach.Say($"{e.Inviter} Invites you to a group");
            }
            var doAlert = activePlayer?.Player?.GroupInviteOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var text = e.Inviter + " Invites you to a group";
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }
        }
    }
}
