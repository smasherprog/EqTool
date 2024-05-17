using EQTool.Models;
using EQToolShared.HubModels;
using System;
using System.Linq;
using static EQTool.Services.LogParser;

namespace EQTool.Services.Parsing
{
    public class LogStartCustomTimer : IEqLogParseHandler
    {
        private readonly string StartTimer = "you say, 'timer start";
        private readonly string StartTimer1 = "you say, 'start timer";

        private readonly LogEvents logEvents;
        public LogStartCustomTimer(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = GetStartTimer(line);
            if (m != null)
            {
                logEvents.Handle(new StartTimerEventArgs { CustomTimer = m });
                return true;
            }
            return false;
        }

        public CustomTimer GetStartTimer(string message)
        {
            var timer = GetStartTimer(message, StartTimer);
            if (timer != null)
            {
                return timer;
            }
            else
            {
                timer = GetStartTimer(message, StartTimer1);
            }

            return timer;
        }

        private static CustomTimer GetStartTimer(string message, string messagetolookfor)
        {
            if (message.ToLower().StartsWith(messagetolookfor))
            {
                var removedstartimer = message.ToLower().Replace(messagetolookfor, string.Empty).Trim();
                var numbersonly = new string(removedstartimer.Where(a => char.IsDigit(a)).ToArray());
                if (int.TryParse(numbersonly, out var minutesint))
                {
                    var nameasstring = minutesint.ToString();
                    var name = removedstartimer.Replace(nameasstring, string.Empty).Trim('\'').Trim();
                    return new CustomTimer
                    {
                        Name = name,
                        DurationInSeconds = minutesint * 60
                    };
                }
            }

            return null;
        }
    }
}
