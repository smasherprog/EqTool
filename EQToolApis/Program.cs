using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Hubs;
using EQToolApis.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Authentication;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.Extensions;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var sqlconstring = builder.Configuration.GetConnectionString("eqtooldb");
var hangfirecon = builder.Configuration.GetConnectionString("HangfireConnection");
builder.Services.AddDbContext<EQToolContext>(opts => opts.UseSqlServer(sqlconstring)).AddScoped<EQToolContext>();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthentication()
    .AddCookie("DiscordCookie", options =>
    {
        options.Cookie.Name = "DiscordSession";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddOAuth("Discord", options =>
    {
        options.SignInScheme = "DiscordCookie";
        options.ClientId = builder.Configuration["DiscordOAuth:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["DiscordOAuth:ClientSecret"] ?? string.Empty;
        options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.CallbackPath = "/signin-discord";
        options.Scope.Add("identify");
        options.Scope.Add("email");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
                using var response = await ctx.Backchannel.SendAsync(request, ctx.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ctx.HttpContext.RequestAborted));
                var root = doc.RootElement;

                var discordId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var username = root.TryGetProperty("username", out var userProp) ? userProp.GetString() : null;
                var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                var avatar = root.TryGetProperty("avatar", out var avatarProp) ? avatarProp.GetString() : null;

                if (ctx.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                {
                    if (discordId != null) identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, discordId));
                    if (username != null) identity.AddClaim(new Claim(ClaimTypes.Name, username));
                    if (email != null) identity.AddClaim(new Claim(ClaimTypes.Email, email));
                    if (avatar != null) identity.AddClaim(new Claim("urn:discord:avatar", avatar));
                }

                if (discordId != null && username != null)
                {
                    using var scope = ctx.HttpContext.RequestServices.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EQToolContext>();
                    var existing = await db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
                    if (existing == null)
                    {
                        db.DiscordUsers.Add(new DiscordUser
                        {
                            DiscordId = discordId,
                            Username = username,
                            Email = email,
                            Avatar = avatar,
                            CreatedAt = DateTime.UtcNow,
                            LastLoginAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existing.Username = username;
                        existing.Email = email;
                        existing.Avatar = avatar;
                        existing.LastLoginAt = DateTime.UtcNow;
                    }
                    await db.SaveChangesAsync();
                }
            }
        };
    });

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthHandler>("ApiToken", _ => { });

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddResponseCaching();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "P99 Pricing Data API",
        Description = "Below are a set of apis that can be used to get pricing data for green and blue servers.",
        Contact = new OpenApiContact
        {
            Name = "Scott",
            Url = new Uri("https://github.com/smasherprog/EqTool")
        }
    });
    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<EnumSchemaFilter>();
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Paths.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddHangfire(configuration => configuration
     .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
     .UseSimpleAssemblyNameTypeSerializer()
     .UseRecommendedSerializerSettings()
     .UseSqlServerStorage(hangfirecon, new SqlServerStorageOptions
     {
         CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
         SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
         QueuePollInterval = TimeSpan.Zero,
         UseRecommendedIsolationLevel = true,
         DisableGlobalLocks = true,
         DashboardJobListLimit = 2
     }));

#if !DEBUG
builder.Services.AddHangfireServer(a =>
{
    a.WorkerCount = 1;
});
#endif 
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
.AddSingleton<ItemDataService>()
.AddSingleton<IDiscordService, DiscordService>()
.AddSingleton<EQToolShared.Discord.DiscordAuctionParse>()
.AddSingleton<DBData>(a =>
{
    var d = new DBData();
    using (var scope = a.CreateScope())
    {
        var dbcontext = scope.ServiceProvider.GetRequiredService<EQToolContext>();
        var rename = dbcontext.EQitemsV2.FirstOrDefault(a => a.ItemName == "Mana Robe");
        if (rename != null)
        {
            rename.ItemName = "Manna Robe";
            dbcontext.SaveChanges();
        }

        dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        d.TotalEQAuctionPlayers = dbcontext.EQAuctionPlayersV2.Count();
        d.TotalUniqueItems = dbcontext.EQitemsV2.Count();
#if DEBUG

        d.ServerData[(int)Servers.Green] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
        };

        d.ServerData[(int)Servers.Blue] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
        };
#else
  
        d.ServerData[(int)Servers.Green] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
            OldestImportTimeStamp = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            RecentImportTimeStamp = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            TotalEQTunnelAuctionItems = dbcontext.EQTunnelAuctionItemsV2.Where(a => a.Server == Servers.Green).Count(),
            TotalEQTunnelMessages = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Green).Count()
        };

        d.ServerData[(int)Servers.Blue] = new ServerDBData
        {
            OrderByDescendingDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault(),
            OrderByDiscordMessageId = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault(),
            OldestImportTimeStamp = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            RecentImportTimeStamp = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
            TotalEQTunnelAuctionItems = dbcontext.EQTunnelAuctionItemsV2.Where(a => a.Server == Servers.Blue).Count(),
            TotalEQTunnelMessages = dbcontext.EQTunnelMessagesV2.Where(a => a.Server == Servers.Blue).Count()
        };
#endif

    }
    return d;
}).AddSingleton(a =>
{
    var d = new PlayerCacheV2();
    using (var scope = a.CreateScope())
    {
        var dbcontext = scope.ServiceProvider.GetRequiredService<EQToolContext>();
        dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        var allplayers = dbcontext.EQAuctionPlayersV2.Select(a => new AuctionPlayer { EQAuctionPlayerId = a.EQAuctionPlayerId, Name = a.Name }).ToList();
        d.Players = allplayers.ToDictionary(a => a.EQAuctionPlayerId);
    }
    return d;
})
.AddSingleton<NoteableNPCCache>()
.AddScoped<UIDataBuild>()
.AddScoped<NotableNpcCacheService>()
.AddScoped<P99WikiApi>()
.AddScoped<NpcTrackingService>();

builder.Services.AddMvc();
builder.Services.AddAntiforgery();
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EQToolContext>();
    db.Database.Migrate();

    var zones = Zones.ZoneInfoMap;
    var dbzones = db.EQZones.ToList();
    var notablenpcs = db.EQNotableNPCs.ToList();
    foreach (var zone in zones)
    {
        if (!dbzones.Any(a => a.Name == zone.Value.Name))
        {
            _ = db.EQZones.Add(new EQZone
            {
                Name = zone.Value.Name
            });
        }
    }
    dbzones = db.EQZones.ToList();
    foreach (var zone in zones)
    {
        var dbzone = dbzones.FirstOrDefault(a => a.Name == zone.Value.Name);
        if (dbzone != null)
        {
            foreach (var npc in zone.Value.NotableNPCs.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                if (!notablenpcs.Any(a => a.Name == npc))
                {
                    _ = db.EQNotableNPCs.Add(new EQNotableNPC
                    {
                        Name = npc,
                        EQZoneId = dbzone.EQZoneId
                    });
                }
            }
        }
    }
    db.SaveChanges();
    scope.ServiceProvider.GetRequiredService<NotableNpcCacheService>().BuildCache();

}
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
EQToolApis.Models.EQItemImageList.Load(Path.Combine(env.WebRootPath, "Content", "imglst.dat"));

app.UseStaticFiles();
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
        Authorization = [new HangFireAuthorizationFilter()]
    })
    .RequireAuthorization("HangfireAccess");
});
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.MapRazorPages();
app.MapHub<MapHub>("/EqToolMap");
app.MapHub<PPHub>("/PP");
var isrelease = false;

#if !DEBUG
isrelease = true;
#endif

if (isrelease)
{
    using (var scope = app.Services.CreateScope())
    {
        var backgroundclient = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Blue, (a) => a.ReadFutureMessages(Servers.Blue), "*/1 * * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Blue, (a) => a.ReadPastMessages(Servers.Blue), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.ThirtyDays), "0 0 * * 0");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixtyDays), "0 0 3 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.NinetyDays), "0 0 9 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixMonths), "0 0 15 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.Year), "0 0 27 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Green, (a) => a.ReadFutureMessages(Servers.Green), "*/1 * * * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Green, (a) => a.ReadPastMessages(Servers.Green), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.ThirtyDays), "0 0 4 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixtyDays), "0 0 6 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.NinetyDays), "0 0 10 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixMonths), "0 0 16 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.Year), "0 0 26 * *");
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQAuctionPlayers), (a) => a.RebuildEQAuctionPlayers(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionItems), (a) => a.RebuildEQTunnelAuctionItems(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionEQTunnelMessages), (a) => a.RebuildEQTunnelAuctionEQTunnelMessages(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQitems), (a) => a.RebuildEQitems(), Cron.Never);

        backgroundclient.AddOrUpdate<NotableNpcCacheService>(nameof(NotableNpcCacheService.BuildCache) + Servers.Green, (a) => a.BuildCache(), "*/20 * * * *");
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildDataGreen), (a) => a.BuildDataGreen(), "*/7 * * * *");
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildDataBlue), (a) => a.BuildDataBlue(), "*/30 * * * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.ItemDupFix), (a) => a.ItemDupFix(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataMaxCleanup), (a) => a.FixOutlierDataMaxCleanup(), "0 0 11 * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataAfterMaxCleanup), (a) => a.FixOutlierDataAfterMaxCleanup(), "0 0 12 * *");
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.DeleteApiLogs), (a) => a.DeleteApiLogs(), "0 6 * * *");

        var runnow = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataGreen(), TimeSpan.FromSeconds(30));
        runnow.Schedule<UIDataBuild>((a) => a.BuildDataBlue(), TimeSpan.FromSeconds(15));
    }
}
else
{
    using (var scope = app.Services.CreateScope())
    {
        var backgroundclient = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Blue, (a) => a.ReadFutureMessages(Servers.Blue), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Blue, (a) => a.ReadPastMessages(Servers.Blue), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.ThirtyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixtyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.NinetyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.SixMonths), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.Year), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Blue + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Blue, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadFutureMessages) + Servers.Green, (a) => a.ReadFutureMessages(Servers.Green), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.ReadPastMessages) + Servers.Green, (a) => a.ReadPastMessages(Servers.Green), Cron.Never);

        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.ThirtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.ThirtyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixtyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixtyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.NinetyDays.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.NinetyDays), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.SixMonths.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.SixMonths), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.Year.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.Year), Cron.Never);
        backgroundclient.AddOrUpdate<DiscordService.DiscordJob>(nameof(DiscordService.DiscordJob.StartItemPricing) + Servers.Green + DiscordService.DiscordJob.PricingDate.AllTime.ToString(), (a) => a.StartItemPricing(Servers.Green, DiscordService.DiscordJob.PricingDate.AllTime), Cron.Never);

        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQAuctionPlayers), (a) => a.RebuildEQAuctionPlayers(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionItems), (a) => a.RebuildEQTunnelAuctionItems(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQTunnelAuctionEQTunnelMessages), (a) => a.RebuildEQTunnelAuctionEQTunnelMessages(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.RebuildEQitems), (a) => a.RebuildEQitems(), Cron.Never);

        backgroundclient.AddOrUpdate<NotableNpcCacheService>(nameof(NotableNpcCacheService.BuildCache) + Servers.Green, (a) => a.BuildCache(), Cron.Never);
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildDataGreen), (a) => a.BuildDataGreen(), Cron.Never);
        backgroundclient.AddOrUpdate<UIDataBuild>(nameof(UIDataBuild.BuildDataBlue), (a) => a.BuildDataBlue(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.ItemDupFix), (a) => a.ItemDupFix(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataMaxCleanup), (a) => a.FixOutlierDataMaxCleanup(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.FixOutlierDataAfterMaxCleanup), (a) => a.FixOutlierDataAfterMaxCleanup(), Cron.Never);
        backgroundclient.AddOrUpdate<SQLIndexRebuild>(nameof(SQLIndexRebuild.DeleteApiLogs), (a) => a.DeleteApiLogs(), Cron.Never);
    }
}
app.Run();
