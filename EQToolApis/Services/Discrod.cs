using EQToolApis.DB;
using EQToolApis.DB.Models;
using Hangfire;
using Microsoft.Extensions.Options;

namespace EQToolApis.Services
{
    public interface IDiscordService
    {
        public void Login();
        public List<DiscordService.Message> ReadMessages(long? lastid);
        public List<DiscordService.Message> ReadMessageHistory(long? lastid);
    }

    public class DiscordServiceOptions
    {
        public string token { get; set; }
    }

    public class DiscordService : IDiscordService
    {
        private readonly HttpClient _client = new();

        private readonly string token;

        public DiscordService(IOptions<DiscordServiceOptions> options)
        {
            token = options.Value.token;
        }

        public class LoginRequest
        {
            public string captcha_key { get; set; } = null;
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
            //if (loginResponse != null)
            //{
            //    return;
            //}
            //var req = new LoginRequest
            //{
            //    login = login,
            //    password = password
            //};
            //var result = _client.PostAsJsonAsync("https://discord.com/api/v9/auth/login", req).Result;
            //var res = result.Content.ReadAsStringAsync().Result;
            //var resobject = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponse>(res);
            //loginResponse = resobject;
            //if (string.IsNullOrWhiteSpace(loginResponse.Token))
            //{
            //    throw new Exception($"Login Failed: {res}");
            //}
        }

        public class embedFields
        {
            public int? Price
            {
                get
                {
                    if (name == "No Price Listed" || name.StartsWith("00") || name.Contains("00000000000000000"))
                    {
                        return null;
                    }

                    _ = float.TryParse(new string(name.Where(a => char.IsDigit(a) || a == '.').ToArray()), out var p);
                    if (name.EndsWith("pp"))
                    {
                        if (p <= 0)
                        {
                            return null;
                        }
                        else if (p > 1000000)
                        {
                            return null;
                        }
                        return (int)p;
                    }
                    else if (name.EndsWith("k"))

                    {
                        var r = (int)(p * 1000);
                        if (r <= 0)
                        {
                            return null;
                        }
                        else if (r > 1000000)
                        {
                            return null;
                        }
                        return r;
                    }
                    return (int)p;
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
                msg.Headers.Add("authorization", token);
                msg.RequestUri = new Uri(url);
                msg.Method = HttpMethod.Get;
                var result = _client.SendAsync(msg).Result;
                var resultstring = result.Content.ReadAsStringAsync().Result;
                return result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? new List<Message>()
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
            }
        }

        public List<Message> ReadMessageHistory(long? lastid)
        {
            var url = "https://discord.com/api/v9/channels/672512233435168784/messages?limit=50";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/672512233435168784/messages?before={lastid.Value}&limit=50";
            }
            using (var msg = new HttpRequestMessage())
            {
                msg.Headers.Add("authorization", token);
                msg.RequestUri = new Uri(url);
                msg.Method = HttpMethod.Get;
                var result = _client.SendAsync(msg).Result;
                var resultstring = result.Content.ReadAsStringAsync().Result;
                return result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? new List<Message>()
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
            }
        }

        public class DiscordJob
        {
            private readonly IBackgroundJobClient backgroundJobClient;
            private readonly IDiscordService discordService;
            private readonly EQToolContext dbcontext;

            public DiscordJob(ILogger<DiscordJob> logger, IDiscordService discordService, EQToolContext dbcontext, IBackgroundJobClient backgroundJobClient)
            {
                this.dbcontext = dbcontext;
                this.backgroundJobClient = backgroundJobClient;
                this.discordService = discordService;
            }

            private void AddMessages(List<Message> messages, EQToolContext dbcontext)
            {
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

            public void ReadFutureMessages()
            {
                discordService.Login();
                var lastidread = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault();
                AddMessages(discordService.ReadMessages(lastidread), dbcontext);
            }

            public void ReadPastMessages()
            {
                discordService.Login();
                var lastidread = dbcontext.EQTunnelMessages.Where(a => a.Server == Servers.Green).Select(a => (long?)a.DiscordMessageId).OrderBy(a => a).FirstOrDefault();
                AddMessages(discordService.ReadMessageHistory(lastidread), dbcontext);
            }

            public void StartItemPricing()
            {
                var ids = new Queue<int>(dbcontext.EQitems.Select(a => a.EQitemId).ToList());
                _ = backgroundJobClient.Enqueue<DiscordJob>(a => a.DoItemPricing(ids));
            }
            public void DoItemPricing(Queue<int> ids)
            {
                discordService.Login();
                var id = ids.Dequeue();
                var item = dbcontext.EQitems.FirstOrDefault(a => a.EQitemId == id);

                var d = DateTimeOffset.UtcNow.AddMonths(-1);
                item.TotalLast30DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d);
                item.TotalLast30DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                d = DateTimeOffset.UtcNow.AddMonths(-2);
                item.TotalLast60DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d);
                item.TotalLast60DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                d = DateTimeOffset.UtcNow.AddMonths(-3);
                item.TotalLast90DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d);
                item.TotalLast90DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                d = DateTimeOffset.UtcNow.AddMonths(-6);
                item.TotalLast6MonthsCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d);
                item.TotalLast6MonthsAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);

                d = DateTimeOffset.UtcNow.AddYears(-1);
                item.TotalLastYearCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d);
                item.TotalLastYearAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);

                item.TotalAuctionCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id);
                item.TotalAuctionAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id).Average(a => a.AuctionPrice) ?? 0);

                _ = backgroundJobClient.Enqueue<DiscordJob>(a => a.DoItemPricing(ids));
            }
        }
    }
}
