using EQTool.Models;
using EQToolShared.HubModels;
using System;
using System.Linq;

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
                logEvents.Handle(new StartTimerEvent { CustomTimer = m, TimeStamp = timestamp });
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
                if (removedstartimer.Contains(":"))
                {
                    var numbersonly = new string(removedstartimer.Where(a => char.IsDigit(a) || a == ':').ToArray());
                    if (int.TryParse(numbersonly.Split(':')[0], out var minutes) && int.TryParse(numbersonly.Split(':')[1], out var seconds))
                    {
                        var firstnumber = removedstartimer.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                        if (firstnumber != -1)
                        {
                            var nameasstring = removedstartimer.Substring(0, firstnumber).Trim();
                            var ts = new TimeSpan(0, minutes, seconds);
                            return new CustomTimer
                            {
                                Name = nameasstring,
                                DurationInSeconds = (int)ts.TotalSeconds
                            };
                        }
                    }
                }
                else
                {
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
            }

            return null;
        }
    }
}
