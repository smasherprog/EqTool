using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EQToolApis.Services
{
    internal interface IDiscordService
    {
        public SocketTextChannel GetSocketTextChannel(ulong channelId);
    }
    public class DiscordServiceOptions
    {
        public string BotToken { get; set; }
    }

    public class DiscordService : IDiscordService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;

        public DiscordService(IOptions<DiscordServiceOptions> options, ILogger<DiscordService> logger)
        {
            _logger = logger;
            _client = new DiscordSocketClient();
            _client.Log += LogDiscord;
            _client.Ready += OnReady;
            _ = _client.LoginAsync(TokenType.Bot, options.Value.BotToken);
            _ = _client.StartAsync();
        }

        public SocketTextChannel GetSocketTextChannel(ulong channelId)
        {
            return _client.GetChannel(channelId) as SocketTextChannel;
        }

        private async Task OnReady()
        {
            var channel = GetSocketTextChannel(672512233435168784);
            var msgs = await channel.GetMessagesAsync(5).FlattenAsync();
            foreach (var msg in msgs)
            {
                Debug.WriteLine(msg);
            }
        }

        private Task LogDiscord(LogMessage msg)
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
