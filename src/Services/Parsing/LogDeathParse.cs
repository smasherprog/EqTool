using EQTool.Models;
using EQTool.Services.Parsing;
using static EQTool.Services.EventsList;

namespace EQTool.Services.Spells.Log
{
    public class LogDeathParse : ILogParser
    {
        private readonly string HasBeenSlainBy = "has been slain by";
        private readonly string Died = "died.";
        private readonly string YouHaveSlain = "You have slain";
        private readonly string YouHaveBeenSlain = "You have been slain";

        private readonly EventsList eventsList;

        public LogDeathParse(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            var nameofthingindex = line.IndexOf(HasBeenSlainBy);
            if (nameofthingindex != -1 || line.EndsWith(Died))
            {
                if (line.StartsWith("Eye of "))
                {
                    return true;
                }

                if (nameofthingindex != -1)
                {
                    var n = line.Substring(0, nameofthingindex).Trim();
                    this.eventsList.Handle(new DeadEventArgs { Name = n });
                    return true;
                }
                else if (!line.Contains(", '") && line.EndsWith(Died))
                {
                    nameofthingindex = line.IndexOf(Died);
                    var n = line.Substring(0, nameofthingindex).Trim();
                    this.eventsList.Handle(new DeadEventArgs { Name = n });
                    return true;
                }
            }
            else if (line.StartsWith(YouHaveSlain))
            {
                var nameofthing = line.Replace(YouHaveSlain, string.Empty).TrimEnd('!').Trim();
                this.eventsList.Handle(new DeadEventArgs { Name = nameofthing });
                return true;
            }
            else if (line.StartsWith(YouHaveBeenSlain))
            {
                this.eventsList.Handle(new DeadEventArgs { Name = EQSpells.SpaceYou });
                return true;

            }

            return false;
        }
    }
}
