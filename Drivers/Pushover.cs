using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Pushover : INotifier {

        private static readonly HttpClient _client = new HttpClient();

        private string _token;
        private string _user;
        private string _device;
        private string _highPriRetries;
        private string _highPriExpire;
        private Action<string> _onError;

        public void Initialize(NotifierConfig config) {
            _token = config.PushoverToken;
            _user = config.PushoverUser;
            _device = config.PushoverDevice;
            _highPriRetries = config.PushoverHighPriRetries;
            _highPriExpire = config.PushoverHighPriExpire;
            _onError = config.OnError;
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

            // retry/expire only apply to emergency priority (2), and Pushover rejects the
            // whole message if they're out of range (retry >= 30, expire 30-10800).
            if (priority == 2) {
                values["retry"] = (int.TryParse(_highPriRetries, out int r) ? Math.Max(r, 30) : 30).ToString();
                values["expire"] = (int.TryParse(_highPriExpire, out int e) ? Math.Min(Math.Max(e, 30), 10800) : 300).ToString();
            }

            try {
                using (var content = new FormUrlEncodedContent(values)) {
                    var response = await _client.PostAsync("https://api.pushover.net/1/messages.json", content);
                    var body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode) {
                        // Prefer Pushover's own error text (the {"errors":[...]} field) over raw JSON.
                        var errors = Regex.Match(body ?? "", "\"errors\":\\[(.*?)\\]");
                        var reason = errors.Success ? errors.Groups[1].Value.Replace("\"", "") : body;
                        _onError?.Invoke($"Pushover rejected the notification (HTTP {(int)response.StatusCode}): {reason}");
                    }
                }
            } catch (Exception ex) {
                _onError?.Invoke($"Pushover request failed: {ex.Message}");
            }
        }

    }
}
