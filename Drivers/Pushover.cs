using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vPilot_Pushover.Drivers {
    internal class Pushover : HttpNotifierBase {

        private string _token;
        private string _user;
        private string _device;
        private string _highPriRetries;
        private string _highPriExpire;

        protected override void Configure(NotifierConfig config) {
            _token = config.PushoverToken;
            _user = config.PushoverUser;
            _device = config.PushoverDevice;
            _highPriRetries = config.PushoverHighPriRetries;
            _highPriExpire = config.PushoverHighPriExpire;
        }

        public override bool HasValidConfig() {
            return !string.IsNullOrWhiteSpace(_token) && !string.IsNullOrWhiteSpace(_user);
        }

        public override async Task SendMessageAsync(string text, string title = "", int priority = 0) {
            var values = new Dictionary<string, string>
            {
                { "token", _token },
                { "user", _user },
                { "title", title },
                { "message", text },
                { "priority", priority.ToString() }
            };

            if (!string.IsNullOrWhiteSpace(_device)) {
                values["device"] = _device;
            }

            // retry/expire only apply to emergency priority (2), and Pushover rejects the
            // whole message if they're out of range (retry >= 30, expire 30-10800).
            if (priority == 2) {
                values["retry"] = (int.TryParse(_highPriRetries, out int r) ? Math.Max(r, 30) : 30).ToString();
                values["expire"] = (int.TryParse(_highPriExpire, out int e) ? Math.Min(Math.Max(e, 30), 10800) : 300).ToString();
            }

            await PostFormAsync("https://api.pushover.net/1/messages.json", values);
        }

        // Prefer Pushover's own error text (the {"errors":[...]} field) over raw JSON.
        protected override string ExtractErrorDetail(string body) {
            var errors = Regex.Match(body ?? "", "\"errors\":\\[(.*?)\\]");
            return errors.Success ? errors.Groups[1].Value.Replace("\"", "") : body;
        }

    }
}
