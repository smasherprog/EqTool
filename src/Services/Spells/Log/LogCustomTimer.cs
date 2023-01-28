using System;
using System.Linq;

namespace EQTool.Services.Spells.Log
{
    public class LogCustomTimer
    {
        private readonly string StartTimer = "you say, 'timer start";
        private readonly string StartTimer1 = "you say, 'start timer";
        private readonly string CancelTimer = "you say, 'timer cancel";
        private readonly string CancelTimer1 = "you say, 'cancel timer";

        public LogCustomTimer()
        {
        }

        public class CustomerTimer
        {
            public string Name { get; set; }
            public int DurationInSeconds { get; set; }
        }

        public CustomerTimer GetStartTimer(string linelog)
        {
            var date = linelog.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var message = linelog.Substring(27).Trim();
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

        private static CustomerTimer GetStartTimer(string message, string messagetolookfor)
        {
            if (message.ToLower().StartsWith(messagetolookfor))
            {
                var removedstartimer = message.ToLower().Replace(messagetolookfor, string.Empty).Trim();
                var numbersonly = new string(removedstartimer.Where(a => char.IsDigit(a)).ToArray());
                if (int.TryParse(numbersonly, out var minutesint))
                {
                    var nameasstring = minutesint.ToString();
                    var name = removedstartimer.Replace(nameasstring, string.Empty).Trim('\'').Trim();
                    return new CustomerTimer
                    {
                        Name = name,
                        DurationInSeconds = minutesint * 60
                    };
                }
            }

            return null;
        }

        public string GetCancelTimer(string linelog)
        {
            var date = linelog.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var message = linelog.Substring(27).Trim();
            var nametoremove = GetCancelTimer(message, CancelTimer);
            if (!string.IsNullOrWhiteSpace(nametoremove))
            {
                return nametoremove;
            }
            else
            {
                nametoremove = GetCancelTimer(message, CancelTimer1);
            }
            return nametoremove;
        }

        private static string GetCancelTimer(string message, string messagetolookfor)
        {
            if (message.ToLower().StartsWith(messagetolookfor))
            {
                var nametoremove = message.ToLower().Replace(messagetolookfor, string.Empty).Trim('\'').Trim().Trim();
                return nametoremove;
            }

            return null;
        }
    }
}
