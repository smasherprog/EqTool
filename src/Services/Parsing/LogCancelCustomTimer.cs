using EQTool.Models;
using System;
using static EQTool.Services.LogParser;

namespace EQTool.Services.Parsing
{
    public class LogCancelCustomTimer : IEqLogParseHandler
    {
        private readonly string CancelTimer = "you say, 'timer cancel";
        private readonly string CancelTimer1 = "you say, 'cancel timer";

        private readonly LogEvents logEvents;
        public LogCancelCustomTimer(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = GetCancelTimer(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new CancelTimerEventArgs { Name = m });
                return true;
            }
            return false;
        }

        public string GetCancelTimer(string message)
        {
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
