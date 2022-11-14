using System;
using System.Diagnostics;

namespace EQTool.Services.Spells.Log
{
    public class LogDeathParse
    {
        private readonly string HasBeenSlainBy = "has been slain by";
        private readonly string YouHaveSlain = "You have slain";

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
            Debug.WriteLine($"DeathParse: " + message);
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

            return null;
        }
    }
}
