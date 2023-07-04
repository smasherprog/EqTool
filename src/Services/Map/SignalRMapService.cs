using EQTool.ViewModels;

namespace EQTool.Services.Map
{
    public class SignalRMapService
    {
        //private HubConnection HubConnection;
        //public event Action<PlayerLocation> PlayerLocationReceived;
        //private readonly LogParser logParser;
        //private readonly ActivePlayer activePlayer;

        public SignalRMapService(LogParser logParser, ActivePlayer activePlayer)
        {
            //this.logParser = logParser;
            //this.activePlayer = activePlayer;
            //this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEventAsync;
            //_ = Task.Factory.StartNew(() =>
            //{
            //    _ = Connect();
            //});
        }

        //private void LogParser_PlayerLocationEventAsync(object sender, LogParser.PlayerLocationEventArgs e)
        //{
        //    //if (e.PlayerInfo.Server == null)
        //    //{
        //    //    return;
        //    //}

        //    //SendPlayerLocation(new PlayerLocation
        //    //{
        //    //    PlayerName = activePlayer.Player.Name,
        //    //    ZoneName = e.PlayerInfo.Zone,
        //    //    Server = e.PlayerInfo.Server.Value,
        //    //    X = e.Location.X,
        //    //    Y = e.Location.Y,
        //    //    Z = e.Location.Z
        //    //});
        //}

        ////for testing from the settings page
        //public void SendPlayerLocation(PlayerLocation playerLocation)
        //{
        //    Debug.WriteLine("SendPlayerLocation");
        //    try
        //    {
        //        _ = HubConnection.SendAsync("SendPlayerLocation", playerLocation);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("SendPlayerLocation Error " + ex.ToString());
        //    }
        //}

        //private async Task Connect()
        //{
        //    if (HubConnection?.State == HubConnectionState.Connected)
        //    {
        //        return;
        //    }
        //    _ = HubConnection?.DisposeAsync();
        //    try
        //    {
        //        HubConnection = new HubConnectionBuilder()
        //            .WithUrl("https://pigparse.org/EqToolMap")
        //            .Build();
        //        _ = HubConnection.On<PlayerLocation>("ReceivePlayerLocation", (playerLocation) =>
        //        {
        //            Debug.WriteLine("playerLocation Received");
        //            PlayerLocationReceived?.Invoke(playerLocation);
        //        }
        //        );
        //        _ = HubConnection.On<PlayerLocation>("ReceivePlayerLeftZone", (playerLocation) =>
        //        {
        //            Debug.WriteLine("ReceivePlayerLeftZone Received");
        //            PlayerLocationReceived?.Invoke(playerLocation);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //    HubConnection.Closed += async (error) =>
        //    {
        //        Debug.WriteLine("Signalr Connection Closed. Retrying");
        //        await Task.Delay(new Random().Next(0, 5) * 1000);
        //        Debug.WriteLine("Signalr Reconnecting .. ");
        //        await HubConnection.StartAsync();
        //    };
        //    Debug.WriteLine("Signalr Connecting .. ");
        //    await HubConnection.StartAsync().ContinueWith(t => { Debug.WriteLine("Signalr Connected!"); });
        //}
    }
}
