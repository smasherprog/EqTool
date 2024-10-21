using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Services.P99LoginMiddlemand;
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
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsClass && !x.IsAbstract))
            {
                if (type.GetInterfaces().Contains(typeof(Models.IEqLogParseHandler)))
                {
                    _ = builder.RegisterType(type).As<Models.IEqLogParseHandler>().SingleInstance();
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
            _ = builder.RegisterType<ViewModels.ActivePlayer>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.SpellWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.LogParser>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.DPSWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ViewModels.ZoneViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.SessionPlayerDamage>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.LoggingService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.PlayerTrackerService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.ZoneActivityTrackingService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.TimersService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Models.SignalrPlayerHub>().As<Models.ISignalrPlayerHub>().SingleInstance();
            _ = builder.RegisterType<Services.AudioService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.SettingsTestRunOverlay>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
