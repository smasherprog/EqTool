using EQToolShared.Enums;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EQToolApis.Hubs
{
    public class PPHub : Hub
    {
        public static readonly ConcurrentDictionary<string, SignalrPlayerV2> connections = new ConcurrentDictionary<string, SignalrPlayerV2>();

        public async Task PlayerLocationEvent(SignalrPlayerV2 playerLocation)
        { 
            if (connections.TryGetValue(Context.ConnectionId, out var player))
            {
                if (player.Zone != playerLocation.Zone || player.Sharing != playerLocation.Sharing)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, player.GroupName);
                    await Groups.AddToGroupAsync(Context.ConnectionId, playerLocation.GroupName);
                    player.Name = playerLocation.Name;
                    player.GuildName = playerLocation.GuildName;  
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

        public async Task DragonRoarEvent(SignalRDragonRoar dragonRoar)
        {
            await Clients.Group(dragonRoar.GroupName).SendAsync("DragonRoarEvent", dragonRoar); 
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
