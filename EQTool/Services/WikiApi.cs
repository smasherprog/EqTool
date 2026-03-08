using EQTool.ViewModels;

namespace EQTool.Services
{
    public class WikiApi
    {
        private readonly ActivePlayer activePlayer;
        public WikiApi(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public string GetData(string name)
        {
            var currentzone = activePlayer?.Player?.Zone;
            try
            {
                var url = $"https://pigparse.azurewebsites.net/api/item/wiki/{name}?zonename={currentzone}";
                var res = App.httpclient.GetAsync(url).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    return response;
                }
            }
            catch
            {
            }

            return string.Empty;
        }
    }
}
