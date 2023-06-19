using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services
{
    public static class DI
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            _ = builder.RegisterType<EQToolSettingsLoad>().AsSelf().SingleInstance();
            _ = builder.Register(a =>
            {
                return a.Resolve<EQToolSettingsLoad>().Load();
            }).AsSelf().SingleInstance();
            _ = builder.RegisterType<AppDispatcher>().As<IAppDispatcher>().SingleInstance();
            _ = builder.RegisterType<SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ParseSpells_spells_us>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SettingsWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQSpells>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ActivePlayer>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SpellWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<LogParser>().AsSelf().SingleInstance();
            _ = builder.RegisterType<DPSWindowViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ZoneViewModel>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SessionPlayerDamage>().AsSelf().SingleInstance();
            _ = builder.RegisterType<LoggingService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<PlayerTrackerService>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ZoneActivityTrackingService>().AsSelf().SingleInstance();


            return builder.Build();
        }
    }
}
