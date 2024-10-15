using Autofac;
using Autofac.Features.ResolveAnything;
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
            _ = builder.RegisterType<EQTool.Services.Parsing.LocationParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.CampParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.PlayerWhoLogParse>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.DPSLogParse>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.LogDeathParse>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.ConLogParse>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.LogCancelCustomTimer>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.LogStartCustomTimer>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.Parsing.CharmBreakParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.OutlierSpellEffectParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.Parsing.SpellCastParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.ResistSpellParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.SpellWornOffOtherParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.SpellWornOffSelfParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.Parsing.QuakeParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.RandomParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.DeathParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.DeathTouchParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.EnrageParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.Parsing.CompleteHealParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.LevParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.InvisParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.FTEParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.FailedFeignParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();

            _ = builder.RegisterType<EQTool.Services.Parsing.GroupInviteParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.LevelLogParse>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();
            _ = builder.RegisterType<EQTool.Services.Parsing.EnterWorldParser>().As<EQTool.Models.IEqLogParseHandler>().SingleInstance();


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
