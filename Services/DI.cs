using Autofac;
using Autofac.Features.ResolveAnything;

namespace EQTool.Services
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
            _ = builder.RegisterType<SpellIcons>().AsSelf().SingleInstance();
            _ = builder.RegisterType<ParseSpells>().AsSelf().SingleInstance();
            return builder.Build();
        }
    }
}
