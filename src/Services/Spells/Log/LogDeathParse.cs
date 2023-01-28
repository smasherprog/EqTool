using EQTool.Models;
using System;

namespace EQTool.Services.Spells.Log
{
    public class LogDeathParse
    {
        private readonly string HasBeenSlainBy = "has been slain by";
        private readonly string YouHaveSlain = "You have slain";
        private readonly string YouHaveBeenSlain = "You have been slain";

        public LogDeathParse()
        {
        }

        public string GetDeadTarget(string linelog)
        {
            var date = linelog.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var message = linelog.Substring(27);
            if (message.Contains(HasBeenSlainBy))
            {
                var nameofthingindex = message.IndexOf(HasBeenSlainBy);
                if (nameofthingindex == -1)
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
