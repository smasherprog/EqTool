using EQToolShared.Enums;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EQToolApis.Hubs
{
    public class MapHub : Hub
    {
        public static readonly ConcurrentDictionary<string, SignalrPlayer> connections = new ConcurrentDictionary<string, SignalrPlayer>();

        public async Task PlayerLocationEvent(SignalrPlayer playerLocation)
        {
            if ((DateTime.UtcNow - playerLocation.TimeStamp).TotalMinutes > 1)
            {
                if (connections.TryRemove(Context.ConnectionId, out var p))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, p.GroupName);
                    await Clients.Group(p.GroupName).SendAsync("PlayerDisconnected", p);
                    Debug.WriteLine($"{p.GroupName}, ReceivePlayerLeftZone {p.Name}, {p.Zone}");
                }
                return;
            }
            if (connections.TryGetValue(Context.ConnectionId, out var player))
            {
                if (player.Zone != playerLocation.Zone || player.MapLocationSharing != playerLocation.MapLocationSharing)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.GroupName);
                    await Groups.AddToGroupAsync(Context.ConnectionId, playerLocation.GroupName);
                    player.Name = playerLocation.Name;
                    player.GuildName = playerLocation.GuildName;
                    player.PlayerClass = playerLocation.PlayerClass;
                    player.MapLocationSharing = playerLocation.MapLocationSharing;
                    player.Server = playerLocation.Server;
                    player.Zone = playerLocation.Zone;
                    player.TrackingDistance = playerLocation.TrackingDistance;
                }
            }
            else if (connections.TryAdd(Context.ConnectionId, playerLocation))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, playerLocation.GroupName);
                Debug.WriteLine($"{playerLocation.GroupName}, TryAdd {playerLocation.Name}, {playerLocation.Zone}");
            }

            await Clients.Group(playerLocation.GroupName).SendAsync("PlayerLocationEvent", playerLocation);
            Debug.WriteLine($"{playerLocation.GroupName}, {playerLocation.Name}, {playerLocation.Zone}, {playerLocation.X}, {playerLocation.Y}, {playerLocation.Z}");
        }
         
        public async Task JoinServerGroup(Servers servers)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, servers.ToString());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (connections.TryRemove(Context.ConnectionId, out var player))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.Server.ToString());
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.GroupName);
                await Clients.Group(player.GroupName).SendAsync("PlayerDisconnected", player);
                Debug.WriteLine($"{player.GroupName}, ReceivePlayerLeftZone {player.Name}, {player.Zone}");
            }

            _ = base.OnDisconnectedAsync(exception);
        }
    }
}
