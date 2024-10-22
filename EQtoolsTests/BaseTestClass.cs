using Autofac;
using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.Enums;
namespace EQtoolsTests
{
    public class BaseTestClass
    {
        protected readonly IContainer container;
        public BaseTestClass()
        {
            container = DI.Init();
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
