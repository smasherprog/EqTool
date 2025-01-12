using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQTool.Services.P99LoginMiddlemand;
using EQTool.ViewModels;
using System;
using System.Linq;

namespace EQTool
{
    public static class DI
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            _ = builder.RegisterType<Services.LogEvents>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.EQToolSettingsLoad>().AsSelf().SingleInstance();
            _ = builder.RegisterType<LoginMiddlemand>().AsSelf().SingleInstance();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass && !x.IsAbstract && x.Namespace?.StartsWith("EQTool") == true).ToList();
            foreach (var type in types)
            {
                if (type.GetInterfaces().Contains(typeof(Models.IEqLogParser)))
                {
                    _ = builder.RegisterType(type).As<Models.IEqLogParser>().SingleInstance();
                }
            }
            types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract && x.Namespace?.StartsWith("EQTool") == true).ToList();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BaseHandler)))
                {
                    _ = builder.RegisterType(type).AsSelf().As<BaseHandler>().SingleInstance();

                }
            }

            _ = builder.Register(a =>
            {
                return a.Resolve<Services.EQToolSettingsLoad>().Load();
            }).AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.AppDispatcher>().As<Services.IAppDispatcher>().SingleInstance();
            _ = builder.RegisterType<Services.SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.ParseSpells_spells_us>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.SettingsWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.EQSpells>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.PlayerPet>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.ActivePlayer>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.SpellWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.LogParser>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.DPSWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.ZoneViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.SessionPlayerDamage>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.LoggingService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<FightHistory>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SpellDurations>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ConsoleViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<DebugOutput>().AsSelf().SingleInstance();

            _ = builder.RegisterType<Services.PlayerTrackerService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.ZoneActivityTrackingService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.TimersService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.SignalrPlayerHub>().SingleInstance();
            _ = builder.RegisterType<Services.SettingsTestRunOverlay>().AsSelf().SingleInstance();
            _ = builder.RegisterType<TextToSpeach>().As<ITextToSpeach>().SingleInstance();

            return builder.Build();
        }
    }
}
