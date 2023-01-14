using System.Collections.Generic;
using System.Diagnostics;

namespace EQTool.Services.Spells.Log
{
    public class ConLogParse
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

        public ConLogParse()
        {

        }

        public string ConMatch(string linelog)
        {
            var message = linelog.Substring(27);
            Debug.WriteLine($"ConLogParse: " + message);
            foreach (var item in ConMessages)
            {
                var indexof = message.IndexOf(item);
                if (indexof != -1)
                {
                    var nameofthis = message.Substring(0, indexof);
                    return nameofthis;
                }
            }
            return string.Empty;
        }
    }
}
