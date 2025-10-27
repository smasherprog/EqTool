using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Diagnostics;
namespace EQtoolsTests
{
    public class BaseTestClass
    {
        protected readonly IContainer container;
        protected readonly ActivePlayer player;
        public BaseTestClass()
        {
            container = DI.Init();
            player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 20,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan",
                Name = "pigy"
            };
        }
    }

    public static class LogParserExtention
    {
        public static void Push(this LogParser logParser, SpellWindowViewModel spellWindowViewModel, string message, DateTime datetime)
        {
            //this parsing is needed because the logger will truncate anything beyond a seconds worth of time
            var d = datetime.ToString("G");
            var logdatetime = DateTime.Parse(d);
            var diff = logdatetime - logParser.LastEntryDateTime;
            Debug.WriteLine($"BEG datetime {logdatetime} logdatetime {logParser.LastEntryDateTime}");
            if (diff.TotalMilliseconds >= 1000)
            {
                Debug.WriteLine($"Fast forwarding {diff.TotalMilliseconds}ms for log line '{message}'");
                var totaltime = diff.TotalMilliseconds;
                var timeslices = totaltime / 100;
                for (var time = 0; time < timeslices; time++)
                {
                    totaltime -= 100;
                    spellWindowViewModel.UpdateTriggers(100);
                }

                if (totaltime > 0)
                {
                    spellWindowViewModel.UpdateTriggers(totaltime);
                }
            }
            logParser.Push(message, datetime);
            Debug.WriteLine($"END datetime {logdatetime} logdatetime {logParser.LastEntryDateTime}");
        }
    }
}
