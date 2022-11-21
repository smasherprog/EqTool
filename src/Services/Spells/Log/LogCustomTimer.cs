using System;
using System.Diagnostics;

namespace EQTool.Services.Spells.Log
{
    public class LogCustomTimer
    {
        private readonly string StartTimer = "you say, 'timer start";
        private readonly string CancelTimer = "you say, 'timer cancel";

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
            Debug.WriteLine($"Custom Timer: " + message);

            if (message.ToLower().StartsWith(StartTimer))
            {
                var removedstartimer = message.ToLower().Replace(StartTimer, string.Empty).Trim();
                var nameindex = removedstartimer.IndexOf(" ");
                if (nameindex != -1)
                {
                    var name = removedstartimer.Substring(0, nameindex);
                    var minutes = removedstartimer.Replace(name, string.Empty).Trim('\'').Trim(); ;
                    if (int.TryParse(minutes, out var minutesint))
                    {
                        return new CustomerTimer
                        {
                            Name = name,
                            DurationInSeconds = minutesint * 60
                        };
                    }
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
            Debug.WriteLine($"Custom Timer: " + message);

            if (message.ToLower().StartsWith(CancelTimer))
            {
                var nametoremove = message.ToLower().Replace(CancelTimer, string.Empty).Trim('\'').Trim().Trim();
                return nametoremove;
            }

            return null;
        }
    }
}
