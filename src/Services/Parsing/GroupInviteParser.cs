using System.Linq;

namespace EQTool.Services
{
    public class GroupInviteParser
    {
        public string Parse(string line)
        {
            if (line.EndsWith(" invites you to join a group."))
            {
                var remainder = line.Replace(" invites you to join a group.", string.Empty);
                var name = remainder.Trim();
                if (name.Contains(' '))
                {
                    return string.Empty;
                }
                return line;
            }

            return null;
        }
    }
}
