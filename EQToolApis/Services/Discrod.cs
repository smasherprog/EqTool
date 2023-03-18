using EQToolApis.DB;
using EQToolApis.DB.Models;
using Microsoft.Extensions.Options;
using static EQToolApis.Services.DiscordService;

namespace EQToolApis.Services
{
    public interface IDiscordService
    {
        public void Login();
        public List<Message> ReadMessages(long? lastid);
    }

    public class DiscordServiceOptions
    {
        public string login { get; set; }
        public string password { get; set; }
    }

    public class DiscordService : IDiscordService
    {
        private readonly HttpClient _client = new();
        private LoginResponse loginResponse;
        private readonly string login;
        private readonly string password;

        public DiscordService(IOptions<DiscordServiceOptions> options)
        {
            login = options.Value.login;
            password = options.Value.password;
        }

        public class LoginRequest
        {
            public string captcha_key { get; set; }
            public string gift_code_sku_id { get; set; }
            public string login { get; set; }
            public string login_source { get; set; }
            public string password { get; set; }
            public bool undelete { get; set; } = false;
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public string user_id { get; set; }
        }

        public void Login()
        {
            if (loginResponse != null)
            {
                return;
            }
            var req = new LoginRequest
            {
                login = login,
                password = password
            };
            var result = _client.PostAsJsonAsync("https://discord.com/api/v9/auth/login", req).Result;
            loginResponse = result.Content.ReadFromJsonAsync<LoginResponse>().Result;
        }

        public class embedFields
        {
            public int? Price
            {
                get
                {
                    if (name == "No Price Listed")
                    {
                        return null;
                    }
                    _ = int.TryParse(new string(name.Where(a => char.IsDigit(a)).ToArray()), out var p);
                    if (name.EndsWith("pp"))
                    {
                        return p;
                    }
                    else if (name.EndsWith("k"))
                    {
                        return p * 1000;
                    }
                    return p;
                }
            }

            public string ItemName => string.IsNullOrWhiteSpace(value) || !value.Contains("[") || !value.Contains("]")
                        ? string.Empty
                        : value.Substring(1, value.IndexOf("]")).Trim(']').Trim('[').Trim();

            public string name { get; set; }
            public string value { get; set; }
        }

        public class MessageEmbed
        {
            public string title { get; set; }
            public AuctionType AuctionType => title.StartsWith("**[ WTB ]**") ? AuctionType.WTB : (title.StartsWith("**[ WTS ]**") ? AuctionType.WTS : AuctionType.BOTH);
            public string AuctionPerson => title[(title.LastIndexOf("**") + 2)..].Trim();
            public DateTimeOffset timestamp { get; set; }
            public List<embedFields> fields { get; set; }
        }

        public class Message
        {
            public long id { get; set; }
            public List<MessageEmbed> embeds { get; set; }
        }

        public List<Message> ReadMessages(long? lastid)
        {
            var url = "https://discord.com/api/v9/channels/672512233435168784/messages?limit=50";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/672512233435168784/messages?after={lastid.Value}&limit=50";
            }
            using (var msg = new HttpRequestMessage())
            {
                msg.Headers.Add("authorization", loginResponse.Token);
                msg.RequestUri = new Uri(url);
                msg.Method = HttpMethod.Get;
                var result = _client.SendAsync(msg).Result;
                var resultstring = result.Content.ReadAsStringAsync().Result;
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    loginResponse = null;
                    return new List<Message>();
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
            }
        }

        public class TimedHostedService : IHostedService, IDisposable
        {
            private readonly ILogger<TimedHostedService> _logger;
            private Timer? _timer = null;
            private readonly IDiscordService discordService;
            private readonly IServiceProvider services;
            private bool Processing = false;

            public TimedHostedService(ILogger<TimedHostedService> logger, IDiscordService discordService, IServiceProvider services)
            {
                this.services = services;
                _logger = logger;
                this.discordService = discordService;
            }

            public Task StartAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation("Timed Hosted Service running.");
                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                return Task.CompletedTask;
            }

            private void DoWork(object? state)
            {
                if (Processing)
                {
                    return;
                }

                Processing = true;
                try
                {
                    discordService.Login();
                    using (var scope = services.CreateScope())
                    {
                        var dbcontext = scope.ServiceProvider.GetRequiredService<EQToolContext>();
                        var lastidread = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault();
                        var messages = discordService.ReadMessages(lastidread);
                        foreach (var item in messages)
                        {
                            var embed = item.embeds.FirstOrDefault();
                            if (embed != null && embed.fields != null)
                            {
                                var eqplayer = dbcontext.EQAuctionPlayers.FirstOrDefault(a => a.Name == embed.AuctionPerson);
                                if (eqplayer == null)
                                {
                                    eqplayer = new EQAuctionPlayer
                                    {
                                        Name = embed.AuctionPerson.Trim()
                                    };
                                    _ = dbcontext.EQAuctionPlayers.Add(eqplayer);
                                    _ = dbcontext.SaveChanges();
                                }

                                var m = new DB.Models.EQTunnelMessage
                                {
                                    AuctionType = embed.AuctionType,
                                    EQAuctionPlayerId = eqplayer.EQAuctionPlayerId,
                                    DiscordMessageId = item.id,
                                    Server = Servers.Green,
                                    TunnelTimestamp = embed.timestamp,
                                    EQTunnelAuctionItems = new List<EQTunnelAuctionItem>()
                                };
                                foreach (var it in embed.fields)
                                {
                                    var eqitem = dbcontext.EQitems.FirstOrDefault(a => a.ItemName == it.ItemName);
                                    if (eqitem == null)
                                    {
                                        eqitem = new EQitem
                                        {
                                            ItemName = it.ItemName
                                        };
                                        _ = dbcontext.EQitems.Add(eqitem);
                                        _ = dbcontext.SaveChanges();
                                    }
                                    m.EQTunnelAuctionItems.Add(new EQTunnelAuctionItem
                                    {
                                        AuctionPrice = it.Price,
                                        EQitemId = eqitem.EQitemId
                                    });
                                }
                                _ = dbcontext.EQTunnelMessages.Add(m);
                            }
                        }
                        _ = dbcontext.SaveChanges();
                    }

                    _logger.LogInformation("Timed Hosted Service is working.");
                }
                finally
                {
                    Processing = false;
                }
            }

            public Task StopAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
                _ = (_timer?.Change(Timeout.Infinite, 0));
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                _timer?.Dispose();
            }
        }
    }
}
