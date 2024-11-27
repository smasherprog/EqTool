using Autofac;
using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.Enums;
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
}
