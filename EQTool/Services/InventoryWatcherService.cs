using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.APIModels.InventoryControllerModels;
using EQToolShared.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class InventoryWatcherService : IDisposable
    {
        private readonly EQToolSettings _settings;
        private readonly ActivePlayer _activePlayer;
        private readonly HttpClient _httpClient = new HttpClient();
        private FileSystemWatcher _watcher;

        public InventoryWatcherService(EQToolSettings settings, ActivePlayer activePlayer)
        {
            _settings = settings;
            _activePlayer = activePlayer;
        }

        public void Start()
        {
            var dir = _settings.DefaultEqDirectory;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return;
            }

            _watcher = new FileSystemWatcher(dir, "*.txt")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileChanged;
            _watcher.Changed += OnFileChanged;
        }

        public void UpdateDirectory()
        {
            Dispose();
            Start();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (string.IsNullOrEmpty(_settings.DiscordId) || string.IsNullOrEmpty(_settings.DiscordApiToken))
            {
                return;
            }

            var characterName = _activePlayer.Player?.Name;
            var server = _activePlayer.Player?.Server;
            if (string.IsNullOrEmpty(characterName) || server == null)
            {
                return;
            }

            var items = TryParseInventoryFile(e.FullPath);
            if (items == null)
            {
                return;
            }

            var request = new InventoryUploadRequest
            {
                CharacterName = characterName,
                Server = server.Value,
                Items = items
            };

            var token = _settings.DiscordApiToken;
            _ = Task.Factory.StartNew(() => PostInventory(request, token));
        }

        private static List<InventoryItemModel> TryParseInventoryFile(string path)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                if (lines.Length < 2)
                {
                    return null;
                }

                var header = lines[0].Split('\t');
                if (header.Length < 5 || header[0] != "Location" || header[1] != "Name" || header[2] != "ID")
                {
                    return null;
                }

                var items = new List<InventoryItemModel>();
                for (var i = 1; i < lines.Length; i++)
                {
                    var parts = lines[i].Split('\t');
                    if (parts.Length < 5)
                    {
                        continue;
                    }

                    if (!int.TryParse(parts[2], out var itemId) ||
                        !int.TryParse(parts[3], out var count) ||
                        !int.TryParse(parts[4], out var slots))
                    {
                        continue;
                    }

                    var locationKey = parts[0].Replace("-", string.Empty);
                    if (!Enum.TryParse<InventoryLocation>(locationKey, ignoreCase: true, out var location))
                    {
                        location = InventoryLocation.Unknown;
                    }

                    items.Add(new InventoryItemModel
                    {
                        Location = location,
                        Name = parts[1],
                        ItemId = itemId,
                        Count = count,
                        Slots = slots
                    });
                }

                return items.Count > 0 ? items : null;
            }
            catch
            {
                return null;
            }
        }

        private void PostInventory(InventoryUploadRequest request, string apiToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                _ = _httpClient.PostAsync("https://pigparse.azurewebsites.net/api/inventory/upload", content).Result;
            }
            catch { }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }
    }
}
