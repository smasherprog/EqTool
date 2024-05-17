using System.Linq;

namespace EQTool.Services.Parsing
{
    public class GroupInviteParser
    {
        public string Parse(string line)
        {
            if (line.EndsWith(" invites you to join a group."))
            {
                var remainder = line.Replace(" invites you to join a group.", string.Empty);
                var name = remainder.Trim();
                return name.Contains(' ') ? string.Empty : line;
            }

            return null;
        }
    }
}
