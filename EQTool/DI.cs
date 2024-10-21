using Autofac;
using Autofac.Features.ResolveAnything;
using EQTool.Services.P99LoginMiddlemand;

namespace EQTool
{
    public static class DI
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();
            _ = builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            _ = builder.RegisterType<Services.LogEvents>().AsSelf().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LocationParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.CampParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.PlayerWhoLogParse>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.HitParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LogDeathParse>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.ConLogParse>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LogCancelCustomTimer>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LogStartCustomTimer>().As<Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<Services.Parsing.CharmBreakParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.OutlierSpellEffectParser>().As<Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<Services.Parsing.SpellCastParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.ResistSpellParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.SpellWornOffOtherParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.SpellWornOffSelfParser>().As<Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<Services.Parsing.QuakeParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.RandomParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            //  _ = builder.RegisterType<Services.Parsing.DeathParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.DeathTouchParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.EnrageParser>().As<Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<Services.Parsing.CompleteHealParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LevParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.InvisParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.FTEParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.FailedFeignParser>().As<Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<Services.Parsing.GroupInviteParser>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.LevelLogParse>().As<Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<Services.Parsing.EnterWorldParser>().As<Models.IEqLogParseHandler>().SingleInstance();


            _ = builder.RegisterType<Services.EQToolSettingsLoad>().AsSelf().SingleInstance();
            _ = builder.RegisterType<LoginMiddlemand>().AsSelf().SingleInstance();

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
