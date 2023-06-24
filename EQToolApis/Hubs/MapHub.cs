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
            var player = new Player
            {
                PlayerName = playerLocation.PlayerName,
                Server = playerLocation.Server,
                ZoneName = playerLocation.ZoneName
            };
            if (!connections.TryAdd(Context.ConnectionId, player))
            {
                if (player.ZoneName != playerLocation.ZoneName)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.GroupName);
                    await Clients.Group(player.GroupName).SendAsync("ReceivePlayerLeftZone", player);
                    player.ZoneName = playerLocation.ZoneName;
                    await Groups.AddToGroupAsync(Context.ConnectionId, player.GroupName);
                    Debug.WriteLine($"{player.GroupName}, ReceivePlayerLeftZone {player.PlayerName}, {player.ZoneName}");
                }
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, player.GroupName);
                _ = connections.TryAdd(Context.ConnectionId, player);
                Debug.WriteLine($"{player.GroupName}, TryAdd {player.PlayerName}, {player.ZoneName}");
            }

            await Clients.Group(player.GroupName).SendAsync("ReceivePlayerLocation", playerLocation);
            Debug.WriteLine($"{player.GroupName}, {playerLocation.PlayerName}, {playerLocation.ZoneName}, {playerLocation.X}, {playerLocation.Y}, {playerLocation.Z}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (connections.TryRemove(Context.ConnectionId, out var player))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.GroupName);
                await Clients.Group(player.GroupName).SendAsync("ReceivePlayerLeftZone", player);
                Debug.WriteLine($"{player.GroupName}, ReceivePlayerLeftZone {player.PlayerName}, {player.ZoneName}");
            }

            _ = base.OnDisconnectedAsync(exception);
        }
    }
}
