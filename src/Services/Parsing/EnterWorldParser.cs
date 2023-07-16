using EQTool.Services.Parsing;
using System;

namespace EQTool.Services
{
    public class EnterWorldParser
    {

        public EnterWorldParser()
        {

        }

        public bool HasEnteredWorld(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }
            if (!line.StartsWith("["))
            {
                return false;
            }
            if (line.Length <= 28)
            {
                return false;
            }

            var date = line.Substring(1, 24);
            var message = line.Substring(27).Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var timestamp = LogFileDateTimeParse.ParseDateTime(date);
            var delta = (DateTime.Now - timestamp).TotalSeconds;
            return delta <= 4 && delta >= 0 && message == "Welcome to EverQuest!";
        }
    }
}
