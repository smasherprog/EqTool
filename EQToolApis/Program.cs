using EQToolApis.DB;
using EQToolApis.Services;
using Microsoft.EntityFrameworkCore;
using static EQToolApis.Services.DiscordService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<EQToolContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("eqtooldb"))).AddScoped<EQToolContext>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<DiscordServiceOptions>(options =>
{
    options.login = builder.Configuration.GetSection("Discord").GetValue<string>("Login");
    options.password = builder.Configuration.GetSection("Discord").GetValue<string>("Password");
})
.AddSingleton<IDiscordService, DiscordService>();
builder.Services.AddHostedService<TimedHostedService>();

builder.Services.AddMvc();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EQToolContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.MapRazorPages();

app.Run();
