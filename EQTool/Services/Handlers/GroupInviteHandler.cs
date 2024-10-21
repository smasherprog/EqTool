using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class GroupInviteHandler : BaseHandler
    {
        public GroupInviteHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.GroupInviteEvent += LogParser_GroupInviteEvent;
        }

        private void LogParser_GroupInviteEvent(object sender, GroupInviteEvent e)
        {
            if (activePlayer?.Player?.GroupInviteAudio == true)
            {
                textToSpeach.Say($"{e.Inviter} Invites you to a group");
            }
        }
    }
}
