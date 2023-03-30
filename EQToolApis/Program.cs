using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<EQToolContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("eqtooldb"))).AddScoped<EQToolContext>();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddResponseCaching();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHangfire(configuration => configuration
     .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
     .UseSimpleAssemblyNameTypeSerializer()
     .UseRecommendedSerializerSettings()
     .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
     {
         CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
         SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
         QueuePollInterval = TimeSpan.Zero,
         UseRecommendedIsolationLevel = true,
         DisableGlobalLocks = true,
         DashboardJobListLimit = 2
     }));
builder.Services.AddHangfireServer(a =>
{
    a.WorkerCount = 1;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HangfireAccess", cfgPolicy =>
    {
        cfgPolicy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser();
    });
});
builder.Services.Configure<DiscordServiceOptions>(options =>
{
    options.token = builder.Configuration.GetValue<string>("DiscordToken");
})
.AddSingleton<IDiscordService, DiscordService>()
.AddSingleton<DBData>(a =>
{
    var d = new DBData();
    using (var scope = a.CreateScope())
    {
        var dbcontext = scope.ServiceProvider.GetRequiredService<EQToolContext>();
        dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        d.TotalEQAuctionPlayers = dbcontext.EQAuctionPlayers.Count();
        d.TotalUniqueItems = dbcontext.EQitems.Count();
        d.ServerData[(int)Servers.Green] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
            OldestImportTimeStamp = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            RecentImportTimeStamp = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            TotalEQTunnelAuctionItems = dbcontext.EQTunnelAuctionItems.Where(a => a.Server == Servers.Green).Count(),
            TotalEQTunnelMessages = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Count()
        };

        d.ServerData[(int)Servers.Blue] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
            OldestImportTimeStamp = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Blue).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            RecentImportTimeStamp = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Blue).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            TotalEQTunnelAuctionItems = dbcontext.EQTunnelAuctionItems.Where(a => a.Server == Servers.Blue).Count(),
            TotalEQTunnelMessages = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Blue).Count()
        };
    }
    return d;
}).AddSingleton<PlayerCache>(a =>
{
    var d = new PlayerCache();
    using (var scope = a.CreateScope())
    {
        var dbcontext = scope.ServiceProvider.GetRequiredService<EQToolContext>();
        dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        var allplayers = dbcontext.EQAuctionPlayers.AsNoTracking().ToList();
        d.Players = allplayers.Select(a => new AuctionPlayer { EQAuctionPlayerId = a.EQAuctionPlayerId, Name = a.Name }).ToDictionary(a => a.EQAuctionPlayerId);
    }
    return d;
})
.AddScoped<UIDataBuild>();

builder.Services.AddMvc();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EQToolContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions()
    {
        Authorization = new List<IDashboardAuthorizationFilter> { }
    })
    .RequireAuthorization("HangfireAccess");
});
app.MapControllers();
app.MapRazorPages();

var isrelease = false;

#if !DEBUG
isrelease = true;
#endif

if (isrelease)
{
    using (var scope = app.Services.CreateScope())
    {
        var backgroundclient = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Blue, (a) => a.ReadFutureMessages(Servers.Blue), "*/2 * * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Blue, (a) => a.ReadPastMessages(Servers.Blue), "*/1 * * * *");

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.ThirtyDays), "0 */1 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixtyDays), "0 8 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.NinetyDays), "30 8 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixMonths), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.Year), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Green, (a) => a.ReadFutureMessages(Servers.Green), "*/2 * * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Green, (a) => a.ReadPastMessages(Servers.Green), "*/1 * * * *");

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.ThirtyDays), "30 */1 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixtyDays), "15 8 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.NinetyDays), "45 8 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixMonths), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.Year), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQAuctionPlayers), (a) => a.RebuildEQAuctionPlayers(), "0 10 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionItems), (a) => a.RebuildEQTunnelAuctionItems(), "20 10 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionEQTunnelMessages), (a) => a.RebuildEQTunnelAuctionEQTunnelMessages(), "35 10 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQitems), (a) => a.RebuildEQitems(), "50 10 * * *");

        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildData) + Servers.Green, (a) => a.BuildData(Servers.Green), "*/7 * * * *");
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildData) + Servers.Blue, (a) => a.BuildData(Servers.Blue), "*/30 * * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.MessageDupFix), (a) => a.MessageDupFix(), Cron.Daily);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.ItemDupFix), (a) => a.ItemDupFix(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataMaxCleanup), (a) => a.FixOutlierDataMaxCleanup(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataAfterMaxCleanup), (a) => a.FixOutlierDataAfterMaxCleanup(), Cron.Never);

        var runnow = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataGreen(), TimeSpan.FromSeconds(20));
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataBlue(), TimeSpan.FromSeconds(10));
    }
}
app.Run();
