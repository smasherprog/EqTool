using Autofac;
using Autofac.Features.ResolveAnything;
using System;
using System.IO;
using System.Linq;

namespace EQtoolsTests
{
    public static class DI
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            _ = builder.Register(a =>
            {
                return new EQTool.Models.EQToolSettings
                {
                    DefaultEqDirectory = string.Empty,
                    EqLogDirectory = string.Empty,
                    BestGuessSpells = true,
                    YouOnlySpells = false,
                    Players = new System.Collections.Generic.List<EQTool.Models.PlayerInfo>(),
                    DpsWindowState = new EQTool.Models.WindowState
                    {
                        Closed = false,
                        State = System.Windows.WindowState.Normal
                    },
                    MapWindowState = new EQTool.Models.WindowState
                    {
                        Closed = false,
                        State = System.Windows.WindowState.Normal
                    },
                    MobWindowState = new EQTool.Models.WindowState
                    {
                        Closed = false,
                        State = System.Windows.WindowState.Normal
                    },
                    SpellWindowState = new EQTool.Models.WindowState
                    {
                        Closed = false,
                        State = System.Windows.WindowState.Normal
                    }
                };
            }).AsSelf().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.LogEvents>().AsSelf().SingleInstance();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass && !x.IsAbstract))
            {
                if (type.GetInterfaces().Contains(typeof(EQTool.Models.IEqLogParseHandler)))
                {
                    _ = builder.RegisterType(type).As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
                }
            }

            _ = builder.RegisterType<EQTool.Services.FakeAppDispatcher>().As<EQTool.Services.IAppDispatcher>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.ParseSpells_spells_us>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.SettingsWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Models.EQSpells>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.ActivePlayer>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.SpellWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.LogParser>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.DPSWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQToolShared.Discord.DiscordAuctionParse>().AsSelf().SingleInstance();

            var b = builder.Build();
            var settings = b.Resolve<EQTool.Models.EQToolSettings>();
            settings.DefaultEqDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return b;
        }
    }
}
