using EQTool.Models;

namespace EQTool.Services.Spells.Log
{
    public class LogDeathParse
    {
        private readonly string HasBeenSlainBy = "has been slain by";
        private readonly string Died = "died.";
        private readonly string YouHaveSlain = "You have slain";
        private readonly string YouHaveBeenSlain = "You have been slain";

        public LogDeathParse()
        {
        }

        public string GetDeadTarget(string message)
        {
            var nameofthingindex = message.IndexOf(HasBeenSlainBy);
            if (nameofthingindex != -1 || message.EndsWith(Died))
            {
                if (message.StartsWith("Eye of "))
                {
                    return string.Empty;
                }

                if (!message.Contains(", '") && message.EndsWith(Died))
                {
                    nameofthingindex = message.IndexOf(Died);
                }
                else
                {
                    return string.Empty;
                }

                var target = message.Substring(0, nameofthingindex).Trim();
                return target;
            }
            else if (message.StartsWith(YouHaveSlain))
            {
                var nameofthing = message.Replace(YouHaveSlain, string.Empty).TrimEnd('!').Trim();
                return nameofthing;
            }
            else if (message.StartsWith(YouHaveBeenSlain))
            {
                return EQSpells.SpaceYou;
            }
            return null;
        }
    }
}
