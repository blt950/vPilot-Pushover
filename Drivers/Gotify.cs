using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Gotify : INotifier {

        private static readonly HttpClient _client = new HttpClient();

        private string _url;
        private string _token;

        public void Initialize(NotifierConfig config) {
            _url = config.GotifyUrl;
            _token = config.GotifyToken;
        }

        public bool HasValidConfig() {
            return _url != null && _token != null;
        }

        public async Task SendMessageAsync(string text, string title = "", int priority = 0) {
            var values = new Dictionary<string, string>
            {
                { "title", title },
                { "message", text },
                { "priority", priority.ToString() }
            };

            string endpoint = $"{_url}/message?token={_token}";
            using (var content = new FormUrlEncodedContent(values)) {
                var response = await _client.PostAsync(endpoint, content);
                await response.Content.ReadAsStringAsync();
            }
        }

    }
}
