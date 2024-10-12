using EQTool.Models;
using System;
using System.Collections.Generic;

namespace EQTool.Services.Parsing
{
    public class ConLogParse : IEqLogParseHandler
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
        private readonly LogEvents logEvents;

        public ConLogParse(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = ConMatch(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new ConEvent { Name = m });
                return true;
            }
            return false;
        }

        public string ConMatch(string message)
        {
            foreach (var item in ConMessages)
            {
                var indexof = message.IndexOf(item);
                if (indexof != -1)
                {
                    var nameofthis = message.Substring(0, indexof);
                    return nameofthis?.Trim();
                }
            }
            return string.Empty;
        }
    }
}
