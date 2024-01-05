using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Models;
using EQToolShared.Discord;
using EQToolShared.Enums;
using Hangfire;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace EQToolApis.Services
{
    public interface IDiscordService
    {
        public void Login();
        public List<Message> ReadMessages(long? lastid, Servers server);
        public List<Message> ReadMessageHistory(long? lastid, Servers server);
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
            ChannelIds[(int)Servers.Green] = "789988380754706523";
            ChannelIds[(int)Servers.Blue] = "789988433220075530";
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

        public List<Message> ReadMessages(long? lastid, Servers server)
        {
            var url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?limit=100";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?after={lastid.Value}&limit=100";
            }
            using (var msg = new HttpRequestMessage())
            {
                msg.Headers.Add("authorization", token);
                msg.RequestUri = new Uri(url);
                msg.Method = HttpMethod.Get;
                var result = _client.SendAsync(msg).Result;
                var resultstring = result.Content.ReadAsStringAsync().Result;
                try
                {
                    return result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? new List<Message>()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + " --- " + resultstring);
                }
            }
        }

        public List<Message> ReadMessageHistory(long? lastid, Servers server)
        {
            var url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?limit=100";
            if (lastid.HasValue)
            {
                url = $"https://discord.com/api/v9/channels/{ChannelIds[(int)server]}/messages?before={lastid.Value}&limit=100";
            }

            using (var msg = new HttpRequestMessage())
            {
                msg.Headers.Add("authorization", token);
                msg.RequestUri = new Uri(url);
                msg.Method = HttpMethod.Get;
                var result = _client.SendAsync(msg).Result;
                var resultstring = result.Content.ReadAsStringAsync().Result;
                try
                {
                    return result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? new List<Message>()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + " --- " + resultstring);
                }
            }
        }
        public class ThrottledItem
        {
            public int ItemId { get; set; }

            public DateTime LastAdd { get; set; }
        }

        public class DiscordJob
        {
            private readonly IBackgroundJobClient backgroundJobClient;
            private readonly IDiscordService discordService;
            private readonly EQToolContext dbcontext;
            private readonly DBData dBData;
            private readonly PlayerCacheV2 playerCache;
            private readonly List<ThrottledItem>[] ThrottleList = new List<ThrottledItem>[(int)(Servers.Blue + 1)];
            private readonly EQToolShared.Discord.DiscordAuctionParse discordAuctionParse;

            public DiscordJob(PlayerCacheV2 playerCache, DBData dBData, IDiscordService discordService, EQToolContext dbcontext, IBackgroundJobClient backgroundJobClient, EQToolShared.Discord.DiscordAuctionParse discordAuctionParse)
            {
                this.discordAuctionParse = discordAuctionParse;
                this.playerCache = playerCache;
                this.dBData = dBData;
                this.dbcontext = dbcontext;
                this.backgroundJobClient = backgroundJobClient;
                this.discordService = discordService;
                for (var i = 0; i < ThrottleList.Length; i++)
                {
                    ThrottleList[i] = new List<ThrottledItem>();
                }
            }

            private List<EQTunnelMessageV2> AddMessages(List<Message> messages, Servers server)
            {
                var messagesinserted = new List<EQTunnelMessageV2>();
                foreach (var item in messages)
                {
                    var discordpricingdata = this.discordAuctionParse.Parse(item.Text);
                    if (discordpricingdata?.Items?.Any() == true)
                    {
                        var eqplayer = dbcontext.EQAuctionPlayersV2.FirstOrDefault(a => a.Name == discordpricingdata.Player && a.Server == server);
                        if (eqplayer == null)
                        {
                            eqplayer = new EQAuctionPlayerV2
                            {
                                Server = server,
                                Name = discordpricingdata.Player
                            };
                            _ = dbcontext.EQAuctionPlayersV2.Add(eqplayer);
                            _ = dbcontext.SaveChanges();
                            playerCache.PlayersLock.EnterWriteLock();
                            try
                            {
                                playerCache.Players.TryAdd(eqplayer.EQAuctionPlayerId, new AuctionPlayer { EQAuctionPlayerId = eqplayer.EQAuctionPlayerId, Name = eqplayer.Name });
                            }
                            finally
                            {
                                playerCache.PlayersLock.ExitWriteLock();
                            }

                            _ = Interlocked.Add(ref dBData.TotalEQAuctionPlayers, 1);
                        }

                        var m = new DB.Models.EQTunnelMessageV2
                        {
                            AuctionType = discordpricingdata.Items.FirstOrDefault().AuctionType,
                            EQAuctionPlayerId = eqplayer.EQAuctionPlayerId,
                            DiscordMessageId = item.id,
                            Server = server,
                            TunnelTimestamp = item.timestamp,
                            EQTunnelAuctionItems = new List<EQTunnelAuctionItemV2>()
                        };

                        foreach (var it in discordpricingdata.Items)
                        {
                            var eqitem = dbcontext.EQitemsV2.FirstOrDefault(a => a.ItemName == it.Name && a.Server == server);
                            if (eqitem == null)
                            {
                                eqitem = new EQitemV2
                                {
                                    ItemName = it.Name,
                                    Server = server
                                };
                                _ = dbcontext.EQitemsV2.Add(eqitem);
                                _ = dbcontext.SaveChanges();
                                _ = Interlocked.Add(ref dBData.TotalUniqueItems, 1);
                            }
                            if (eqitem.TotalWTBLast90DaysCount > 3000)
                            {
                                var iteminthrottlelist = ThrottleList[(int)server].FirstOrDefault(a => a.ItemId == eqitem.EQitemId);
                                if (iteminthrottlelist == null)
                                {
                                    iteminthrottlelist = new ThrottledItem { ItemId = eqitem.EQitemId, LastAdd = DateTime.Now };
                                    ThrottleList[(int)server].Add(iteminthrottlelist);
                                }
                                else if ((DateTime.Now - iteminthrottlelist.LastAdd).TotalMinutes < 5)
                                {
                                    continue;
                                }
                                iteminthrottlelist.LastAdd = DateTime.Now;
                            }

                            var auctionitem = new EQTunnelAuctionItemV2
                            {
                                Server = server,
                                AuctionPrice = it.Price,
                                EQitemId = eqitem.EQitemId
                            };
                            //  ignore bogus prices
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
                        _ = dbcontext.EQTunnelMessagesV2.Add(m);
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
                var lastidread = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId ?? (server == Servers.Green ? 1186054170009145345 : 1186055073739055265);
                var possiblemessages2 = discordService.ReadMessages(lastidread, server);
                var messages2 = AddMessages(possiblemessages2, server);
                if (messages2.Any())
                {
                    var id = messages2.Select(a => a.DiscordMessageId).OrderByDescending(a => a).FirstOrDefault();
                    var oldid = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId;
                    lock (dBData)
                    {
                        dBData.ServerData[(int)server].RecentImportTimeStamp = messages2.Select(a => a.TunnelTimestamp).OrderByDescending(a => a).FirstOrDefault();
                        dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.HasValue
                            ? Math.Max(dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.Value, id)
                            : id;
                        return messages2.Count + $" messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId}";
                    }
                }
                else if (possiblemessages2.Any())
                {
                    var id = possiblemessages2.Select(a => a.id).OrderByDescending(a => a).FirstOrDefault();
                    var oldid = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId;
                    lock (dBData)
                    {
                        dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId = dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.HasValue
                            ? Math.Max(dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId.Value, id)
                            : id;
                        return $"0 messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDescendingDiscordMessageId}";
                    }
                }
                return messages2.Count + " messages added";
            }

            [AutomaticRetry(Attempts = 0), DisplayName("ReadPastMessages {0}")]
            public string ReadPastMessages(Servers server)
            {
                discordService.Login();
                var lastidread = dBData.ServerData[(int)server].OrderByDiscordMessageId ?? (server == Servers.Green ? 1186054170009145345 : 1186055073739055265);
                var possiblemessages = discordService.ReadMessageHistory(lastidread, server);
                var messages = AddMessages(possiblemessages, server);
                if (messages.Any())
                {
                    var id = messages.Select(a => a.DiscordMessageId).OrderBy(a => a).FirstOrDefault();
                    lock (dBData)
                    {
                        dBData.ServerData[(int)server].OldestImportTimeStamp = messages.Select(a => a.TunnelTimestamp).OrderBy(a => a).FirstOrDefault();
                        var oldid = dBData.ServerData[(int)server].OrderByDiscordMessageId;
                        dBData.ServerData[(int)server].OrderByDiscordMessageId = dBData.ServerData[(int)server].OrderByDiscordMessageId.HasValue
                            ? Math.Min(dBData.ServerData[(int)server].OrderByDiscordMessageId.Value, id)
                            : id;
                        return messages.Count + $" messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDiscordMessageId}";
                    }
                }
                //else if (possiblemessages.Any())
                //{
                //    var id = possiblemessages.Select(a => a.id).OrderBy(a => a).FirstOrDefault();
                //    var oldid = dBData.ServerData[(int)server].OrderByDiscordMessageId;
                //    lock (dBData)
                //    {
                //        dBData.ServerData[(int)server].OrderByDiscordMessageId = dBData.ServerData[(int)server].OrderByDiscordMessageId.HasValue
                //            ? Math.Min(dBData.ServerData[(int)server].OrderByDiscordMessageId.Value, id)
                //            : id;
                //        return $"0 messages added. {oldid} -> {dBData.ServerData[(int)server].OrderByDiscordMessageId}";
                //    }
                //}

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
                var ids = new Queue<int>(dbcontext.EQitemsV2.Where(a => a.Server == server).Select(a => a.EQitemId).ToList());
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
                var items = dbcontext.EQitemsV2.Where(a => itemids.Contains(a.EQitemId)).ToList();
                foreach (var item in items)
                {
                    var d = DateTimeOffset.UtcNow.AddMonths(-1);
                    var id = item.EQitemId;
                    if (pricingDate == PricingDate.ThirtyDays)
                    {
                        item.TotalWTSLast30DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast30DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-2);
                    if (pricingDate == PricingDate.SixtyDays)
                    {
                        item.TotalWTSLast60DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast60DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-3);
                    if (pricingDate == PricingDate.NinetyDays)
                    {
                        item.TotalWTSLast90DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast90DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-6);
                    if (pricingDate == PricingDate.SixMonths)
                    {
                        item.TotalWTSLast6MonthsCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLast6MonthsAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddYears(-1);
                    if (pricingDate == PricingDate.Year)
                    {
                        item.TotalWTSLastYearCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSLastYearAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTS && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    if (pricingDate == PricingDate.AllTime)
                    {
                        item.TotalWTSAuctionCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS);
                        item.TotalWTSAuctionAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS).Average(a => a.AuctionPrice) ?? 0);
                    }

                    item.LastWTSSeen = dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTS).Select(b => (DateTimeOffset?)b.EQTunnelMessage.TunnelTimestamp).OrderByDescending(b => b).FirstOrDefault();

                    d = DateTimeOffset.UtcNow.AddMonths(-1);
                    if (pricingDate == PricingDate.ThirtyDays)
                    {
                        item.TotalWTBLast30DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast30DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.Server == server && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-2);
                    if (pricingDate == PricingDate.SixtyDays)
                    {
                        item.TotalWTBLast60DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast60DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-3);
                    if (pricingDate == PricingDate.NinetyDays)
                    {
                        item.TotalWTBLast90DaysCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast90DaysAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddMonths(-6);
                    if (pricingDate == PricingDate.SixMonths)
                    {
                        item.TotalWTBLast6MonthsCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLast6MonthsAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    d = DateTimeOffset.UtcNow.AddYears(-1);
                    if (pricingDate == PricingDate.Year)
                    {
                        item.TotalWTBLastYearCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBLastYearAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.TunnelTimestamp >= d && a.EQTunnelMessage.AuctionType == AuctionType.WTB && a.AuctionPrice.HasValue).Average(a => a.AuctionPrice) ?? 0);
                    }

                    if (pricingDate == PricingDate.AllTime)
                    {
                        item.TotalWTBAuctionCount = dbcontext.EQTunnelAuctionItemsV2.Count(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB);
                        item.TotalWTBAuctionAverage = (int)(dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB).Average(a => a.AuctionPrice) ?? 0);
                    }

                    item.LastWTBSeen = dbcontext.EQTunnelAuctionItemsV2.Where(a => a.EQitemId == id && a.EQTunnelMessage.AuctionType == AuctionType.WTB).Select(b => (DateTimeOffset?)b.EQTunnelMessage.TunnelTimestamp).OrderByDescending(b => b).FirstOrDefault();
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
