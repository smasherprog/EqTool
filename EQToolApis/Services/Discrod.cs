using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Models;
using EQToolShared.Enums;
using Hangfire;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace EQToolApis.Services
{
    public interface IDiscordService
    {
        public void Login();
        public List<DiscordService.Message> ReadMessages(long? lastid, Servers server);
        public List<DiscordService.Message> ReadMessageHistory(long? lastid, Servers server);
    }

    public class DiscordServiceOptions
    {
        public string token { get; set; } = string.Empty;
    }

    public class DiscordService : IDiscordService
    {
        private readonly HttpClient _client = new();

        private readonly string token;

        private readonly string[] ChannelIds = new string[(int)(Servers.Blue + 1)];
        public DiscordService(IOptions<DiscordServiceOptions> options)
        {
            token = options.Value.token;
            ChannelIds[(int)Servers.Green] = "672512233435168784";
            ChannelIds[(int)Servers.Blue] = "720860598362963998";
        }

        public class LoginRequest
        {
            public string captcha_key { get; set; } = string.Empty;
            public string gift_code_sku_id { get; set; } = string.Empty;
            public string login { get; set; } = string.Empty;
            public string login_source { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
            public bool undelete { get; set; } = false;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string user_id { get; set; } = string.Empty;
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

            public string? name { get; set; }
            public string? value { get; set; }
        }

        public class MessageEmbed
        {
            public string? title { get; set; }
            public AuctionType AuctionType => title.StartsWith("**[ WTB ]**") ? AuctionType.WTB : (title.StartsWith("**[ WTS ]**") ? AuctionType.WTS : AuctionType.BOTH);
            public string AuctionPerson => title[(title.LastIndexOf("**") + 2)..].Trim();
            public DateTimeOffset timestamp { get; set; }
            public List<embedFields>? fields { get; set; }
        }

        public class Message
        {
            public long id { get; set; }
            public List<MessageEmbed>? embeds { get; set; }
        }

        public List<Message> ReadMessages(long? lastid, Servers server)
        {
            var url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?limit=50";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?after={lastid.Value}&limit=50";
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

        public List<Message> ReadMessageHistory(long? lastid, Servers server)
        {
            var url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?limit=50";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?before={lastid.Value}&limit=50";
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
            private readonly DBData dBData;
            private readonly PlayerCache playerCache;
            public DiscordJob(PlayerCache playerCache, DBData dBData, IDiscordService discordService, EQToolContext dbcontext, IBackgroundJobClient backgroundJobClient)
            {
                this.playerCache = playerCache;
                this.dBData = dBData;
                this.dbcontext = dbcontext;
                this.backgroundJobClient = backgroundJobClient;
                this.discordService = discordService;
            }

            private List<EQTunnelMessage> AddMessages(List<Message> messages, Servers server)
            {
                var itemsadded = 0;
                var messagesinserted = new List<EQTunnelMessage>();
                foreach (var item in messages)
                {
                    var embed = item.embeds.FirstOrDefault();
                    if (embed != null && embed.fields != null)
                    {
                        foreach (var it in embed.fields)
                        {
                            if (it.name.Contains("000000000000000000000"))
                            {
                                continue;/// bad data pricing can be off
                            }
                        }

                        var eqplayer = dbcontext.EQAuctionPlayers.FirstOrDefault(a => a.Name == embed.AuctionPerson && a.Server == server);
                        if (eqplayer == null)
                        {
                            eqplayer = new EQAuctionPlayer
                            {
                                Server = server,
                                Name = embed.AuctionPerson.Trim()
                            };
                            _ = dbcontext.EQAuctionPlayers.Add(eqplayer);
                            _ = dbcontext.SaveChanges();
                            playerCache.PlayersLock.EnterWriteLock();
                            try
                            {
                                playerCache.Players.Add(eqplayer.EQAuctionPlayerId, new AuctionPlayer { EQAuctionPlayerId = eqplayer.EQAuctionPlayerId, Name = eqplayer.Name });
                            }
                            finally
                            {
                                playerCache.PlayersLock.ExitWriteLock();
                            }

                            _ = Interlocked.Add(ref dBData.TotalEQAuctionPlayers, 1);
                        }

                        var m = new DB.Models.EQTunnelMessage
                        {
                            AuctionType = embed.AuctionType,
                            EQAuctionPlayerId = eqplayer.EQAuctionPlayerId,
                            DiscordMessageId = item.id,
                            Server = server,
                            TunnelTimestamp = embed.timestamp,
                            EQTunnelAuctionItems = new List<EQTunnelAuctionItem>()
                        };
                        foreach (var it in embed.fields)
                        {
                            itemsadded += 1;
                            var eqitem = dbcontext.EQitems.FirstOrDefault(a => a.ItemName == it.ItemName && a.Server == server);
                            if (eqitem == null)
                            {
                                eqitem = new EQitem
                                {
                                    ItemName = it.ItemName,
                                    Server = server
                                };
                                _ = dbcontext.EQitems.Add(eqitem);
                                _ = dbcontext.SaveChanges();
                                _ = Interlocked.Add(ref dBData.TotalUniqueItems, 1);
                            }
                            var auctionitem = new EQTunnelAuctionItem
                            {
                                Server = server,
                                AuctionPrice = it.Price,
                                EQitemId = eqitem.EQitemId
                            };
                            //ignore bogus prices
                            if (eqitem.TotalWTSLast6MonthsCount > 100 && eqitem.TotalWTSLast6MonthsAverage > 0)
                            {
                                if (it.Price > eqitem.TotalWTSLast6MonthsAverage * 7)
                                {
                                    auctionitem.AuctionPrice = null;
                                }
                                else if (it.Price < eqitem.TotalWTSLast6MonthsAverage * .1)
                                {
                                    auctionitem.AuctionPrice = null;
                                }
                            }
                            m.EQTunnelAuctionItems.Add(auctionitem);
                            _ = Interlocked.Add(ref dBData.ServerData[(int)server].TotalEQTunnelAuctionItems, 1);
                        }
                        _ = Interlocked.Add(ref dBData.ServerData[(int)server].TotalEQTunnelMessages, 1);
                        _ = dbcontext.EQTunnelMessages.Add(m);
                        messagesinserted.Add(m);
                    }
                }
                _ = dbcontext.SaveChanges();
                return messagesinserted;
            }

            [AutomaticRetry(Attempts = 0), DisplayName("ReadFutureMessages {0}")]
            public string ReadFutureMessages(Servers server)
            {
                discordService.Login();
                var lastidread = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId;
                var messages = AddMessages(discordService.ReadMessages(lastidread, server), server);
                if (messages.Any())
                {
                    var id = messages.Select(a => a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault();
                    var oldid = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.Value;
                    lock (dBData)
                    {
                        dBData.ServerData[(int)server].RecentImportTimeStamp = messages.Select(a => a.TunnelTimestamp).OrderByDescending(a => a).FirstOrDefault();
                        dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.HasValue
                            ? Math.Max(dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.Value, id)
                            : id;
                        return messages.Count + $" messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId}";
                    }
                }

                return messages.Count + " messages added";
            }

            [AutomaticRetry(Attempts = 0), DisplayName("ReadPastMessages {0}")]
            public string ReadPastMessages(Servers server)
            {
                discordService.Login();
                var lastidread = dBData.ServerData[(int)server].OrderByDiscordMessageId;
                var messages = AddMessages(discordService.ReadMessageHistory(lastidread, server), server);
                if (messages.Any())
                {
                    var id = messages.Select(a => a.DiscordMessageId).OrderBy(a => a).FirstOrDefault();
                    lock (dBData)
                    {
                        dBData.ServerData[(int)server].OldestImportTimeStamp = messages.Select(a => a.TunnelTimestamp).OrderBy(a => a).FirstOrDefault();
                        var oldid = dBData.ServerData[(int)server].OrderByDiscordMessageId.Value;
                        dBData.ServerData[(int)server].OrderByDiscordMessageId = dBData.ServerData[(int)server].OrderByDiscordMessageId.HasValue
                            ? Math.Min(dBData.ServerData[(int)server].OrderByDiscordMessageId.Value, id)
                            : id;
                        return messages.Count + $" messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDiscordMessageId}";
                    }
                }

                return messages.Count + " messages added";
            }

            public enum PricingDate
            {
                ThirtyDays,
                SixtyDays,
                NinetyDays,
                SixMonths,
                Year,
                AllTime
            }

            [AutomaticRetry(Attempts = 0), DisplayName("Build Pricing {0} {1}")]
            public void StartItemPricing(Servers server, PricingDate pricingDate)
            {
                var ids = new Queue<int>(dbcontext.EQitems.Where(a => a.Server == server).Select(a => a.EQitemId).ToList());
                _ = backgroundJobClient.Enqueue<DiscordJob>(a => a.DoItemPricing(server, ids, pricingDate));
            }

            private static IEnumerable<T> DequeueChunk<T>(Queue<T> queue, int chunkSize)
            {
                for (var i = 0; i < chunkSize && queue.Count > 0; i++)
                {
                    yield return queue.Dequeue();
                }
            }

            public string DoItemPricing(Servers server, Queue<int> ids, PricingDate pricingDate)
            {
                discordService.Login();
                var itemids = DequeueChunk(ids, 50).ToList();
                var items = dbcontext.EQitems.Where(a => itemids.Contains(a.EQitemId)).ToList();
                foreach (var item in items)
                {
                    var d = DateTimeOffset.UtcNow.AddMonths(-1);
                    var id = item.EQitemId;
                    if (pricingDate == PricingDate.ThirtyDays)
                    {
                        item.TotalWTSLast30DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast30DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-2);
                    if (pricingDate == PricingDate.SixtyDays)
                    {
                        item.TotalWTSLast60DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast60DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-3);
                    if (pricingDate == PricingDate.NinetyDays)
                    {
                        item.TotalWTSLast90DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast90DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-6);
                    if (pricingDate == PricingDate.SixMonths)
                    {
                        item.TotalWTSLast6MonthsCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast6MonthsAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddYears(-1);
                    if (pricingDate == PricingDate.Year)
                    {
                        item.TotalWTSLastYearCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLastYearAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    if (pricingDate == PricingDate.AllTime)
                    {
                        item.TotalWTSAuctionCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSAuctionAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS).Average(a => a.AuctionPrice) ?? 0);
                    }

                    item.LastWTSSeen = dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS).Select(b => (DateTimeOffset?)b.EQTunnelMessage.TunnelTimestamp).OrderByDescending(b => b).FirstOrDefault();

                    d = DateTimeOffset.UtcNow.AddMonths(-1);
                    if (pricingDate == PricingDate.ThirtyDays)
                    {
                        item.TotalWTBLast30DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast30DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.Server == server && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-2);
                    if (pricingDate == PricingDate.SixtyDays)
                    {
                        item.TotalWTBLast60DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast60DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-3);
                    if (pricingDate == PricingDate.NinetyDays)
                    {
                        item.TotalWTBLast90DaysCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast90DaysAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-6);
                    if (pricingDate == PricingDate.SixMonths)
                    {
                        item.TotalWTBLast6MonthsCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast6MonthsAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddYears(-1);
                    if (pricingDate == PricingDate.Year)
                    {
                        item.TotalWTBLastYearCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLastYearAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    if (pricingDate == PricingDate.AllTime)
                    {
                        item.TotalWTBAuctionCount = dbcontext.EQTunnelAuctionItems.Count(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBAuctionAverage = (int)(dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB).Average(a => a.AuctionPrice) ?? 0);
                    }

                    item.LastWTBSeen = dbcontext.EQTunnelAuctionItems.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB).Select(b => (DateTimeOffset?)b.EQTunnelMessage.TunnelTimestamp).OrderByDescending(b => b).FirstOrDefault();
                    _ = dbcontext.SaveChanges();
                }

                if (ids.Any())
                {
                    _ = backgroundJobClient.Enqueue<DiscordJob>(a => a.DoItemPricing(server, ids, pricingDate));
                }

                return $"Worked on {itemids.Count} ";
            }
        }
    }
}
