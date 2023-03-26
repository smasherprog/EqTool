using EQToolApis.DB;
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
builder.Services.AddHangfireServer();
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
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Blue, (a) => a.ReadPastMessages(Servers.Blue), "*/2 * * * *");

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.ThirtyDays), "0 */1 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixtyDays), "0 */3 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.NinetyDays), "0 */6 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixMonths), "0 20 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.Year), Cron.Daily);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Weekly);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Green, (a) => a.ReadFutureMessages(Servers.Green), "*/2 * * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Green, (a) => a.ReadPastMessages(Servers.Green), "*/2 * * * *");

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.ThirtyDays), "0 */1 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixtyDays), "0 */3 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.NinetyDays), "0 */6 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixMonths), "0 20 * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.Year), Cron.Daily);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Weekly);

        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQAuctionPlayers), (a) => a.RebuildEQAuctionPlayers(), "20 */20 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionItems), (a) => a.RebuildEQTunnelAuctionItems(), "25 */20 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionEQTunnelMessages), (a) => a.RebuildEQTunnelAuctionEQTunnelMessages(), "30 */20 * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQitems), (a) => a.RebuildEQitems(), "35 */20 * * *");

        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildData) + Servers.Green, (a) => a.BuildData(Servers.Green), "*/7 * * * *");
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildSummaryData), (a) => a.BuildSummaryData(), "0 */1 * * *");
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildData) + Servers.Blue, (a) => a.BuildData(Servers.Blue), "*/30 * * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixDups), (a) => a.FixDups(), Cron.Never);

        var runnow = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataGreen(), TimeSpan.FromSeconds(20));
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataBlue(), TimeSpan.FromSeconds(10));
        runnow.Schedule<UIDataBuild>((a) => a.BuildSummaryData(), TimeSpan.FromSeconds(5));
    }
}
app.Run();
