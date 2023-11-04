using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EQTool.Models
{

    public interface ISignalrPlayerHub
    {
        event EventHandler<SignalrPlayer> PlayerLocationEvent;
        event EventHandler<SignalrPlayer> PlayerDisconnected;
        void PushPlayerLocationEvent(SignalrPlayer player);
        void PushPlayerDisconnected(SignalrPlayer player);
    }

    public class SignalrPlayerHub : ISignalrPlayerHub
    {
        private readonly HubConnection connection;
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;
        private readonly IAppDispatcher appDispatcher;

        {
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            var url = "https://localhost:7056/EqToolMap";
#if !DEBUG
            url ="https://www.pigparse.org/EqToolMap";

#endif
            url = "https://www.pigparse.org/EqToolMap";
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
        connection.On("PlayerLocationEvent", (SignalrPlayer p) =>
            {
            });
            connection.On("PlayerDisconnected", (SignalrPlayer p) =>
            {
            });

            connection.Closed += async(error) =>
              {
                  await Task.Delay(new Random().Next(0, 5) * 1000);
                  ConnectWithRetryAsync(connection).Wait();
    };
            try
            {
                Task.Factory.StartNew(() =>
                {
                    ConnectWithRetryAsync(connection).Wait();
});
            }
            catch (Exception ex)
            {
    Debug.WriteLine(ex.ToString());
}

this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
        }

        public event EventHandler<SignalrPlayer> PlayerLocationEvent;
public event EventHandler<SignalrPlayer> PlayerDisconnected;

private static async Task<bool> ConnectWithRetryAsync(HubConnection connection)
{
    while (true)
    {
        try
        {
            Debug.WriteLine("Beg StartAsync");
            await connection.StartAsync();
            Debug.WriteLine("Connected StartAsync");
            return true;
        }
        catch
        {
            Debug.WriteLine("Failed StartAsync");
            await Task.Delay(5000);
        }
    }
}

public void PushPlayerDisconnected(SignalrPlayer p)
{
    if (!(p.Server == this.activePlayer?.Player?.Server && p.Name == this.activePlayer?.Player?.Name))
    {
        Debug.WriteLine($"PlayerDisconnected {p.Name}");
        this.appDispatcher.DispatchUI(() =>
        {
            PlayerDisconnected?.Invoke(this, p);
        });
    }
}

public void PushPlayerLocationEvent(SignalrPlayer p)
{
    if (!(p.Server == this.activePlayer?.Player?.Server && p.Name == this.activePlayer?.Player?.Name))
    {
        Debug.WriteLine($"PlayerLocationEvent {p.Name}");
        this.appDispatcher.DispatchUI(() =>
        {
            PlayerLocationEvent?.Invoke(this, p);
        });
    }
}

private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
{
    if (this.activePlayer?.Player?.Server != null)
    {
        connection.InvokeAsync("PlayerLocationEvent", new SignalrPlayer
        {
            Zone = this.activePlayer.Player.Zone,
            GuildName = this.activePlayer.Player.GuildName,
            PlayerClass = this.activePlayer.Player.PlayerClass,
            Server = this.activePlayer.Player.Server.Value,
            MapLocationSharing = this.activePlayer.Player.MapLocationSharing.Value,
            Name = this.activePlayer.Player.Name,
            X = e.Location.X,
            Y = e.Location.Y,
            Z = e.Location.Z
        });
    }
}
    }
}
