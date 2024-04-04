using EQTool.Services.Parsing;
using System.Linq;

namespace EQTool.Services
{
    public class GroupInviteParser : ILogParser
    {
        private readonly EventsList eventsList;

        public GroupInviteParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string message, string previousline)
        {
            if (message.EndsWith(" invites you to join a group."))
            {
                var remainder = message.Replace(" invites you to join a group.", string.Empty);
                var name = remainder.Trim();
                if (name.Contains(' '))
                {
                    return false;
                }
                this.eventsList.HandleGroupInvite(message);
                return true;
            }

            return false;
        }
    }
}
