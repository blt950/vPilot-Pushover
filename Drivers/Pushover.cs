using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Pushover : INotifier {

        private static readonly HttpClient _client = new HttpClient();

        private string _token;
        private string _user;
        private string _device;

        public void Initialize(NotifierConfig config) {
            _token = config.PushoverToken;
            _user = config.PushoverUser;
            _device = config.PushoverDevice;
        }

        public bool HasValidConfig() {
            return _token != null && _user != null;
        }

        public async Task SendMessageAsync(string text, string title = "", int priority = 0) {
            var values = new Dictionary<string, string>
            {
                { "token", _token },
                { "user", _user },
                { "title", title },
                { "message", text },
                { "priority", priority.ToString() },
                { "device", _device ?? "" }
            };

            using (var content = new FormUrlEncodedContent(values)) {
                var response = await _client.PostAsync("https://api.pushover.net/1/messages.json", content);
                await response.Content.ReadAsStringAsync();
            }
        }

    }
}
