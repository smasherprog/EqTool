using System.Collections.Generic;

namespace EQTool.Services.Parsing
{
    public class ConLogParse : ILogParser
    {
        private readonly List<string> ConMessages = new List<string>()
        {
            "regards you as an ally",
            "looks upon you warmly",
            "kindly considers you",
            "judges you amiably",
            "regards you indifferently",
            "looks your way apprehensively",
            "glowers at you dubiously",
            "glares at you threateningly",
            "scowls at you, ready to attack"
        };

        private readonly EventsList eventsList;

        public ConLogParse(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line, string previousline)
        {
            foreach (var item in ConMessages)
            {
                var indexof = line.IndexOf(item);
                if (indexof != -1)
                {
                    var nameofthis = line.Substring(0, indexof);
                    this.eventsList.Handle(new EventsList.ConEventArgs { Name = nameofthis?.Trim() });
                    return true;
                }
            }

            return false;
        }
    }
}
