using Autofac;
using EQTool.Models;
using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System.Collections.Generic;
namespace EQtoolsTests
{
    public class BaseTestClass
    {
        protected readonly IContainer container;
        public BaseTestClass()
        {
            container = DI.Init();
            _ = container.Resolve<IEnumerable<BaseHandler>>();
            _ = container.Resolve<IEnumerable<IEqLogParseHandler>>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan",
                Name = "pigy"
            };
        }
    }
}
