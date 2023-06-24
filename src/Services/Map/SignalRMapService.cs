using EQTool.Models;
using EQToolShared.HubModels;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Map
{
    public class SignalRMapService
    {
        private readonly HubConnection HubConnection;
        public event Action<PlayerLocation> PlayerLocationReceived;
        public SignalRMapService(HubConnection hubConnection)
        {
            HubConnection = hubConnection;

            HubConnection.On<PlayerLocation>("ReceivePlayerLocation", (playerLocation) => PlayerLocationReceived?.Invoke(playerLocation));
        }

        public async Task Connect()
        {
            await HubConnection.StartAsync();
        }

        public async Task SendPlayerLocation(PlayerLocation playerLocation)
        {
            await HubConnection.SendAsync("SendPlayerLocation", playerLocation);
        }
    }
}
