using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class LogDeathParse : IEqLogParseHandler
    {
        private readonly string HasBeenSlainBy = "has been slain by";
        private readonly string Died = "died.";
        private readonly string YouHaveSlain = "You have slain";
        private readonly string YouHaveBeenSlain = "You have been slain";

        private readonly LogEvents logEvents;

        public LogDeathParse(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = GetDeadTarget(line);
            if (!string.IsNullOrEmpty(m))
            {
                logEvents.Handle(new DeadEvent { Name = m });
                return true;
            }
            return false;
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

                if (nameofthingindex != -1)
                {
                    return message.Substring(0, nameofthingindex).Trim();
                }
                else if (!message.Contains(", '") && message.EndsWith(Died))
                {
                    nameofthingindex = message.IndexOf(Died);
                    return message.Substring(0, nameofthingindex).Trim();
                }
                else
                {
                    return string.Empty;
                }

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
