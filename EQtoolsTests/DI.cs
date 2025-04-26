using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQtoolsTests.Fakes;
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
                    },
                    Triggers = new System.Collections.Generic.List<Trigger>()
                };
            }).AsSelf().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.LogEvents>().AsSelf().SingleInstance();


            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass && !x.IsAbstract && x.Namespace?.StartsWith("EQTool") == true).ToList();
            foreach (var type in types)
            {
                if (type.GetInterfaces().Contains(typeof(EQTool.Models.IEqLogParser)))
                {
                    _ = builder.RegisterType(type).As<EQTool.Models.IEqLogParser>().SingleInstance();
                }
            }
            types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract && x.Namespace?.StartsWith("EQTool") == true).ToList();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BaseHandler)))
                {
                    _ = builder.RegisterType(type).As<BaseHandler>().SingleInstance();
                }
            }

            _ = builder.RegisterType<AppDispatcherFake>().As<EQTool.Services.IAppDispatcher>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.ParseSpells_spells_us>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.SettingsWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Models.EQSpells>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.ActivePlayer>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.SpellWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.MobInfoComponents.PetViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.MobInfoComponents.MobInfoViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.MobInfoComponents.MobInfoManagementViewModel>().AsSelf().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.LogParser>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.ViewModels.DPSWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQToolShared.Discord.DiscordAuctionParse>().AsSelf().SingleInstance();
            _ = builder.RegisterType<TextToSpeachFake>().As<ITextToSpeach>().SingleInstance();
            _ = builder.RegisterType<FightHistory>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SpellDurations>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.IO.FileReader>().As<EQTool.Services.IO.IFileReader>().SingleInstance();

            _ = builder.RegisterType<Pets>().AsSelf().SingleInstance();

            var b = builder.Build();
            var settings = b.Resolve<EQTool.Models.EQToolSettings>();
            settings.DefaultEqDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return b;
        }
    }
}
