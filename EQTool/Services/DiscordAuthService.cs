using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class DiscordAuthResult
    {
        public string Username { get; set; }
        public string DiscordId { get; set; }
        public string ApiToken { get; set; }
    }

    public class DiscordAuthService
    {
        private static int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public Task<DiscordAuthResult> LoginAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var port = GetAvailablePort();
                var prefix = $"http://127.0.0.1:{port}/";

                var httpListener = new HttpListener();
                httpListener.Prefixes.Add(prefix);
                httpListener.Start();

                var loginUrl = $"https://pigparse.azurewebsites.net/Account/Login?desktop_port={port}";
                Process.Start(new ProcessStartInfo { FileName = loginUrl, UseShellExecute = true });

                var context = httpListener.GetContext();
                httpListener.Stop();

                var responseHtml = "<html><body style='font-family:sans-serif;text-align:center;padding-top:60px'>" +
                                   "<h2>Login successful!</h2><p>You can close this window and return to Pigparse.</p>" +
                                   "</body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseHtml);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/html";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                var query = context.Request.QueryString;
                return new DiscordAuthResult
                {
                    Username = query["username"] ?? string.Empty,
                    DiscordId = query["discord_id"] ?? string.Empty,
                    ApiToken = query["api_token"] ?? string.Empty
                };
            });
        }
    }
}
