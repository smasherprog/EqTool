using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System.IO;

namespace EQToolTests
{
    public static class DI
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            _ = builder.Register(a =>
            {
                return a.Resolve<EQToolSettingsLoad>().Load();
            }).AsSelf().SingleInstance();
            _ = builder.RegisterType<FakeAppDispatcher>().As<IAppDispatcher>().SingleInstance();
            _ = builder.RegisterType<SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ParseSpells_spells_us>().AsSelf().SingleInstance();
            _ = builder.RegisterType<SettingsWindowData>().AsSelf().SingleInstance();
            _ = builder.RegisterType<EQSpells>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ActivePlayer>().AsSelf().SingleInstance();

            var b = builder.Build();
            var settings = b.Resolve<EQToolSettings>();
            settings.DefaultEqDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return b;
        }
    }
}
