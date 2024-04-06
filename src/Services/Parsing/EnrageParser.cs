using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class EnrageParser : ILogParser
    {
        public class EnrageEvent
        {
            public string NpcName { get; set; }
        }

        private readonly EventsList eventsList;

        public EnrageParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line.EndsWith(" has become ENRAGED.", System.StringComparison.OrdinalIgnoreCase))
            {
                var npcname = line.Replace(" has become ENRAGED.", string.Empty).Trim();
                eventsList.Handle(new EnrageEvent { NpcName = npcname });
                return true;
            }

            return false;
        }
    }
}
