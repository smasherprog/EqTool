using EQToolShared.HubModels;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EQToolApis.Hubs
{
    public class MapHub : Hub
    {
        private static readonly ConcurrentDictionary<string, Player> connections = new ConcurrentDictionary<string, Player>();

        public async Task SendPlayerLocation(PlayerLocation playerLocation)
        {
            if (connections.TryGetValue(Context.ConnectionId, out var player))
            {
                if (player.ZoneName != playerLocation.ZoneName)
                {
                    await Clients.Group($"{player.Server}-{player.ZoneName}").SendAsync("ReceivePlayerLeftZone", player);
                    Debug.WriteLine($"{player.Server}-{player.ZoneName}, ReceivePlayerLeftZone {player.PlayerName}, {player.ZoneName}");
                }
            }
            else
            {
                player = new Player
                {
                    PlayerName = playerLocation.PlayerName,
                    Server = playerLocation.Server,
                    ZoneName = playerLocation.ZoneName
                };
                _ = connections.TryAdd(Context.ConnectionId, player);
                Debug.WriteLine($"{player.Server}-{player.ZoneName}, TryAdd {player.PlayerName}, {player.ZoneName}");
            }

            await Clients.Group($"{playerLocation.Server}-{playerLocation.ZoneName}").SendAsync("ReceivePlayerLocation", playerLocation);
            Debug.WriteLine($"{playerLocation} > {playerLocation.Server}, {playerLocation.PlayerName}, {playerLocation.ZoneName}, {playerLocation.X}, {playerLocation.Y}, {playerLocation.Z}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (connections.TryGetValue(Context.ConnectionId, out var player))
            {
                await Clients.Group($"{player.Server}-{player.ZoneName}").SendAsync("ReceivePlayerLeftZone", player);
                Debug.WriteLine($"{player.Server}-{player.ZoneName}, ReceivePlayerLeftZone {player.PlayerName}, {player.ZoneName}");
            }

            _ = base.OnDisconnectedAsync(exception);
        }
    }
}
